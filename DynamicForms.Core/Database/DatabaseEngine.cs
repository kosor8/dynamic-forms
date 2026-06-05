using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using DynamicForms.Core.Models;

namespace DynamicForms.Core.Database
{
    public class DatabaseEngine
    {
        private readonly string connectionString;

        public DatabaseEngine()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DynamicForms");
            Directory.CreateDirectory(appDataPath);
            string dbPath = Path.Combine(appDataPath, "forms.db");
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                string createFormsTable = @"
                    CREATE TABLE IF NOT EXISTS Forms (
                        Id TEXT PRIMARY KEY,
                        Title TEXT NOT NULL,
                        StructureJson TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        Status TEXT NOT NULL DEFAULT 'Draft'
                    );";

                string createSubmissionsTable = @"
                    CREATE TABLE IF NOT EXISTS Submissions (
                        Id TEXT PRIMARY KEY,
                        FormId TEXT NOT NULL,
                        SubmittedAt TEXT NOT NULL,
                        FOREIGN KEY(FormId) REFERENCES Forms(Id) ON DELETE CASCADE
                    );";

                string createSubmissionValuesTable = @"
                    CREATE TABLE IF NOT EXISTS SubmissionValues (
                        Id TEXT PRIMARY KEY,
                        SubmissionId TEXT NOT NULL,
                        ElementId TEXT NOT NULL,
                        Value TEXT NOT NULL,
                        FOREIGN KEY(SubmissionId) REFERENCES Submissions(Id) ON DELETE CASCADE
                    );";

                using (var command = new SqliteCommand(createFormsTable, connection)) command.ExecuteNonQuery();
                using (var command = new SqliteCommand(createSubmissionsTable, connection)) command.ExecuteNonQuery();
                using (var command = new SqliteCommand(createSubmissionValuesTable, connection)) command.ExecuteNonQuery();
            }
        }

        public void SaveFormStructure(string formId, string title, List<FormElement> elements)
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            string structureJson = JsonSerializer.Serialize(elements, options);
            string createdAt = DateTime.UtcNow.ToString("o");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string checkQuery = "SELECT COUNT(*) FROM Forms WHERE Id = @Id";
                bool exists = false;
                using (var checkCmd = new SqliteCommand(checkQuery, connection))
                {
                    checkCmd.Parameters.AddWithValue("@Id", formId);
                    exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;
                }

                if (exists)
                {
                    string updateQuery = "UPDATE Forms SET Title = @Title, StructureJson = @StructureJson WHERE Id = @Id";
                    using (var updateCmd = new SqliteCommand(updateQuery, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@Id", formId);
                        updateCmd.Parameters.AddWithValue("@Title", title);
                        updateCmd.Parameters.AddWithValue("@StructureJson", structureJson);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string insertQuery = "INSERT INTO Forms (Id, Title, StructureJson, CreatedAt, Status) VALUES (@Id, @Title, @StructureJson, @CreatedAt, 'Draft')";
                    using (var insertCmd = new SqliteCommand(insertQuery, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", formId);
                        insertCmd.Parameters.AddWithValue("@Title", title);
                        insertCmd.Parameters.AddWithValue("@StructureJson", structureJson);
                        insertCmd.Parameters.AddWithValue("@CreatedAt", createdAt);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void PublishForm(string formId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("UPDATE Forms SET Status = 'Published' WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", formId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteForm(string formId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmdPragma = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    cmdPragma.ExecuteNonQuery();
                }
                using (var cmd = new SqliteCommand("DELETE FROM Forms WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", formId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Dictionary<string, string> GetSavedForms()
        {
            var forms = new Dictionary<string, string>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("SELECT Id, Title FROM Forms ORDER BY CreatedAt DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            forms[reader.GetString(0)] = reader.GetString(1);
                        }
                    }
                }
            }
            return forms;
        }

        public Dictionary<string, string> GetPublishedForms()
        {
            var forms = new Dictionary<string, string>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("SELECT Id, Title FROM Forms WHERE Status = 'Published' ORDER BY CreatedAt DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            forms[reader.GetString(0)] = reader.GetString(1);
                        }
                    }
                }
            }
            return forms;
        }

        public List<FormElement> LoadFormStructure(string formId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("SELECT StructureJson FROM Forms WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", formId);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string json = result.ToString();
                        return JsonSerializer.Deserialize<List<FormElement>>(json) ?? new List<FormElement>();
                    }
                }
            }
            return new List<FormElement>();
        }

        public void SaveFormSubmission(string formId, Dictionary<string, object> answers)
        {
            string submissionId = Guid.NewGuid().ToString();
            string submittedAt = DateTime.UtcNow.ToString("o");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqliteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    string insertSubmission = "INSERT INTO Submissions (Id, FormId, SubmittedAt) VALUES (@Id, @FormId, @SubmittedAt)";
                    using (var cmdSub = new SqliteCommand(insertSubmission, connection, transaction))
                    {
                        cmdSub.Parameters.AddWithValue("@Id", submissionId);
                        cmdSub.Parameters.AddWithValue("@FormId", formId);
                        cmdSub.Parameters.AddWithValue("@SubmittedAt", submittedAt);
                        cmdSub.ExecuteNonQuery();
                    }

                    string insertValue = "INSERT INTO SubmissionValues (Id, SubmissionId, ElementId, Value) VALUES (@Id, @SubmissionId, @ElementId, @Value)";
                    foreach (var answer in answers)
                    {
                        string valueStr = "";
                        if (answer.Value != null)
                        {
                            if (answer.Value is IEnumerable<string> list)
                                valueStr = string.Join(", ", list);
                            else
                                valueStr = answer.Value.ToString() ?? "";
                        }

                        using (var cmdVal = new SqliteCommand(insertValue, connection, transaction))
                        {
                            cmdVal.Parameters.AddWithValue("@Id", Guid.NewGuid().ToString());
                            cmdVal.Parameters.AddWithValue("@SubmissionId", submissionId);
                            cmdVal.Parameters.AddWithValue("@ElementId", answer.Key);
                            cmdVal.Parameters.AddWithValue("@Value", valueStr);
                            cmdVal.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
            }
        }

        public DataTable GetSubmissions(string formId)
        {
            var dt = new DataTable();
            dt.Columns.Add("SubmissionId", typeof(string));
            dt.Columns.Add("Gönderim Tarihi", typeof(string));

            var elements = LoadFormStructure(formId);
            var questionElements = new List<FormElement>();

            foreach (var element in elements)
            {
                if (element.Type != ElementType.TextBlock)
                {
                    questionElements.Add(element);
                    string colName = element.Title;
                    int counter = 1;
                    while (dt.Columns.Contains(colName))
                    {
                        colName = $"{element.Title} ({counter++})";
                    }
                    dt.Columns.Add(colName, typeof(string));
                }
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT s.Id AS SubmissionId, s.SubmittedAt, sv.ElementId, sv.Value
                    FROM Submissions s
                    LEFT JOIN SubmissionValues sv ON s.Id = sv.SubmissionId
                    WHERE s.FormId = @FormId
                    ORDER BY s.SubmittedAt DESC";

                var submissionRows = new Dictionary<string, (string SubmittedAt, Dictionary<string, string> Values)>();

                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FormId", formId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string subId = reader.GetString(0);
                            string submittedAt = reader.GetString(1);
                            string elementId = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            string val = reader.IsDBNull(3) ? "" : reader.GetString(3);

                            if (!submissionRows.ContainsKey(subId))
                            {
                                string dateStr = submittedAt;
                                if (DateTime.TryParse(submittedAt, out DateTime parsedDate))
                                {
                                    dateStr = parsedDate.ToLocalTime().ToString("g");
                                }
                                submissionRows[subId] = (dateStr, new Dictionary<string, string>());
                            }

                            if (!string.IsNullOrEmpty(elementId))
                            {
                                submissionRows[subId].Values[elementId] = val;
                            }
                        }
                    }
                }

                foreach (var kvp in submissionRows)
                {
                    string subId = kvp.Key;
                    var (submittedAt, values) = kvp.Value;

                    var row = dt.NewRow();
                    row["SubmissionId"] = subId;
                    row["Gönderim Tarihi"] = submittedAt;

                    int colIndex = 2;
                    foreach (var element in questionElements)
                    {
                        string val = "";
                        if (values.TryGetValue(element.Id, out string elementVal))
                        {
                            val = elementVal;
                        }
                        row[colIndex] = val;
                        colIndex++;
                    }
                    dt.Rows.Add(row);
                }
            }

            return dt;
        }

        public Dictionary<string, string> GetSubmissionDetails(string submissionId)
        {
            var details = new Dictionary<string, string>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                string formId = "";
                using (var cmd = new SqliteCommand("SELECT FormId FROM Submissions WHERE Id = @Id", connection))
                {
                    cmd.Parameters.AddWithValue("@Id", submissionId);
                    formId = cmd.ExecuteScalar()?.ToString() ?? "";
                }
                
                if (string.IsNullOrEmpty(formId)) return details;

                var elements = LoadFormStructure(formId);
                var values = new Dictionary<string, string>();
                using (var cmd = new SqliteCommand("SELECT ElementId, Value FROM SubmissionValues WHERE SubmissionId = @SubmissionId", connection))
                {
                    cmd.Parameters.AddWithValue("@SubmissionId", submissionId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values[reader.GetString(0)] = reader.GetString(1);
                        }
                    }
                }

                foreach (var element in elements)
                {
                    if (element.Type != ElementType.TextBlock)
                    {
                        string val = "";
                        values.TryGetValue(element.Id, out val);
                        details[element.Title] = val ?? "";
                    }
                }
            }
            return details;
        }
    }
}
