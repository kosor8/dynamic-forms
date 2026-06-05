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

                var lblRows = new Label { Text = "Satırlar (virgülle ayırın):", AutoSize = true, Location = new Point(0, yPos) };
                panel.Controls.Add(lblRows);
                yPos += 20;
                var txtRows = new TextBox { Text = string.Join(", ", Rows), Width = 400, Location = new Point(0, yPos) };
                txtRows.Leave += (_, _) => { Rows = new List<string>(txtRows.Text.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)); for(int i=0;i<Rows.Count;i++) Rows[i]=Rows[i].Trim(); onUpdate?.Invoke(); };
                panel.Controls.Add(txtRows);
                yPos += 30;

                var lblCols = new Label { Text = "Sütunlar (virgülle ayırın):", AutoSize = true, Location = new Point(0, yPos) };
                panel.Controls.Add(lblCols);
                yPos += 20;
                var txtCols = new TextBox { Text = string.Join(", ", Columns), Width = 400, Location = new Point(0, yPos) };
                txtCols.Leave += (_, _) => { Columns = new List<string>(txtCols.Text.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)); for(int i=0;i<Columns.Count;i++) Columns[i]=Columns[i].Trim(); onUpdate?.Invoke(); };
                panel.Controls.Add(txtCols);
                yPos += 40;
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
