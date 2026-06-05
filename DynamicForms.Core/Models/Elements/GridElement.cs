using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public enum GridType { RadioGrid, CheckBoxGrid }

    public class GridElement : FormElement
    {
        public GridType SubType { get; set; } = GridType.RadioGrid;
        public List<string> Rows { get; set; } = new List<string> { "Satır 1" };
        public List<string> Columns { get; set; } = new List<string> { "Sütun 1", "Sütun 2" };

        public GridElement()
        {
            Type = ElementType.Grid;
            Title = "Seçme Tablosu";
        }

        public override Control RenderControl(bool isDesignerMode, bool isSelected = false, System.Action? onUpdate = null, System.Action? onDelete = null)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel, isSelected, onUpdate);

            if (isSelected)
            {
                var cbSubType = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 250,
                    Location = new Point(0, yPos)
                };
                cbSubType.Items.AddRange(System.Enum.GetNames(typeof(GridType)));
                cbSubType.SelectedItem = SubType.ToString();
                cbSubType.SelectedIndexChanged += (_, _) => { SubType = System.Enum.Parse<GridType>(cbSubType.SelectedItem?.ToString() ?? "RadioGrid"); onUpdate?.Invoke(); };
                panel.Controls.Add(cbSubType);
                yPos += 35;

                var lblRows = new Label { Text = "Satırlar:", AutoSize = true, Location = new Point(0, yPos) };
                panel.Controls.Add(lblRows);

                var lblCols = new Label { Text = "Sütunlar:", AutoSize = true, Location = new Point(250, yPos) };
                panel.Controls.Add(lblCols);
                yPos += 25;

                int maxCount = System.Math.Max(Rows.Count, Columns.Count);
                for (int i = 0; i < maxCount; i++)
                {
                    if (i < Rows.Count)
                    {
                        int rIdx = i;
                        var txtRow = new TextBox { Text = Rows[i], Width = 180, Location = new Point(0, yPos) };
                        txtRow.TextChanged += (_, _) => { Rows[rIdx] = txtRow.Text; };
                        panel.Controls.Add(txtRow);

                        if (Rows.Count > 1)
                        {
                            var btnRemRow = new Button { Text = "✖", Size = new Size(25, 25), Location = new Point(185, yPos - 1), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(0) };
                            btnRemRow.FlatAppearance.BorderSize = 0;
                            btnRemRow.Click += (_, _) => { Rows.RemoveAt(rIdx); onUpdate?.Invoke(); };
                            panel.Controls.Add(btnRemRow);
                        }
                    }

                    if (i < Columns.Count)
                    {
                        int cIdx = i;
                        var txtCol = new TextBox { Text = Columns[i], Width = 180, Location = new Point(250, yPos) };
                        txtCol.TextChanged += (_, _) => { Columns[cIdx] = txtCol.Text; };
                        panel.Controls.Add(txtCol);

                        if (Columns.Count > 1)
                        {
                            var btnRemCol = new Button { Text = "✖", Size = new Size(25, 25), Location = new Point(435, yPos - 1), FlatStyle = FlatStyle.Flat, ForeColor = Color.Gray, Cursor = Cursors.Hand, TextAlign = ContentAlignment.MiddleCenter, Padding = new Padding(0) };
                            btnRemCol.FlatAppearance.BorderSize = 0;
                            btnRemCol.Click += (_, _) => { Columns.RemoveAt(cIdx); onUpdate?.Invoke(); };
                            panel.Controls.Add(btnRemCol);
                        }
                    }
                    yPos += 30;
                }

                var btnAddRow = new Button { Text = "Satır Ekle", AutoSize = true, Location = new Point(0, yPos), FlatStyle = FlatStyle.Flat, ForeColor = Color.DodgerBlue, Cursor = Cursors.Hand };
                btnAddRow.FlatAppearance.BorderSize = 0;
                btnAddRow.Click += (_, _) => { Rows.Add($"Satır {Rows.Count + 1}"); onUpdate?.Invoke(); };
                panel.Controls.Add(btnAddRow);

                var btnAddCol = new Button { Text = "Sütun Ekle", AutoSize = true, Location = new Point(250, yPos), FlatStyle = FlatStyle.Flat, ForeColor = Color.DodgerBlue, Cursor = Cursors.Hand };
                btnAddCol.FlatAppearance.BorderSize = 0;
                btnAddCol.Click += (_, _) => { Columns.Add($"Sütun {Columns.Count + 1}"); onUpdate?.Invoke(); };
                panel.Controls.Add(btnAddCol);
                yPos += 35;
            }

            // Header row
            int xStart = 120;
            for (int c = 0; c < Columns.Count; c++)
            {
                var lbl = new Label
                {
                    Text = Columns[c],
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9f),
                    Location = new Point(xStart + c * 80, yPos)
                };
                panel.Controls.Add(lbl);
            }
            yPos += 22;

            // Data rows
            for (int r = 0; r < Rows.Count; r++)
            {
                var rowLabel = new Label
                {
                    Text = Rows[r],
                    AutoSize = true,
                    Location = new Point(0, yPos + 2)
                };
                panel.Controls.Add(rowLabel);

                for (int c = 0; c < Columns.Count; c++)
                {
                    Control ctrl;
                    if (SubType == GridType.CheckBoxGrid)
                        ctrl = new CheckBox { AutoSize = true, FlatStyle = FlatStyle.Flat };
                    else
                        ctrl = new RadioButton { AutoSize = true, FlatStyle = FlatStyle.Flat };

                    ctrl.Location = new Point(xStart + c * 80, yPos);
                    ctrl.Enabled = !isDesignerMode;
                    ctrl.Name = $"{Id}_{r}_{c}";
                    panel.Controls.Add(ctrl);
                }
                yPos += 28;
            }

            yPos = AddFooterControls(panel, yPos, isSelected, onUpdate, onDelete);

            panel.Size = new Size(500, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var answers = new List<string>();
            for (int r = 0; r < Rows.Count; r++)
            {
                var rowAnswers = new List<string>();
                for (int c = 0; c < Columns.Count; c++)
                {
                    var found = renderedControl.Controls.Find($"{Id}_{r}_{c}", true);
                    if (found.Length > 0)
                    {
                        if (found[0] is CheckBox chk && chk.Checked) rowAnswers.Add(Columns[c]);
                        else if (found[0] is RadioButton rb && rb.Checked) rowAnswers.Add(Columns[c]);
                    }
                }
                if (rowAnswers.Count > 0)
                    answers.Add($"{Rows[r]}: [{string.Join(", ", rowAnswers)}]");
            }
            return string.Join(" | ", answers);
        }
    }
}
