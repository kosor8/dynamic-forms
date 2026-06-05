using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public enum ScaleType { Linear }

    public class ScaleElement : FormElement
    {
        public ScaleType SubType { get; set; } = ScaleType.Linear;
        public int MinValue { get; set; } = 1;
        public int MaxValue { get; set; } = 5;
        public string MinLabel { get; set; } = "";
        public string MaxLabel { get; set; } = "";

        public ScaleElement()
        {
            Type = ElementType.Scale;
            Title = "Ölçek Sorusu";
        }

        public override Control RenderControl(bool isDesignerMode, bool isSelected = false, System.Action? onUpdate = null, System.Action? onDelete = null)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel, isSelected, onUpdate);

            if (isSelected)
            {
                var cbMin = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50, Location = new Point(0, yPos) };
                cbMin.Items.AddRange(new object[] { 0, 1 });
                cbMin.SelectedItem = MinValue;
                cbMin.SelectedIndexChanged += (_, _) => { MinValue = (int)cbMin.SelectedItem; onUpdate?.Invoke(); };

                var lblTo = new Label { Text = "ile", AutoSize = true, Location = new Point(60, yPos + 4) };

                var cbMax = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 50, Location = new Point(90, yPos) };
                for (int i = 2; i <= 10; i++) cbMax.Items.Add(i);
                cbMax.SelectedItem = MaxValue;
                cbMax.SelectedIndexChanged += (_, _) => { MaxValue = (int)cbMax.SelectedItem; onUpdate?.Invoke(); };

                var lblTo2 = new Label { Text = "arası", AutoSize = true, Location = new Point(150, yPos + 4) };

                panel.Controls.Add(cbMin);
                panel.Controls.Add(lblTo);
                panel.Controls.Add(cbMax);
                panel.Controls.Add(lblTo2);
                yPos += 35;

                var lblMinText = new Label { Text = cbMin.SelectedItem?.ToString(), Width = 20, Location = new Point(0, yPos + 4) };
                var txtMinLabel = new TextBox { Text = MinLabel, PlaceholderText = "Etiket (isteğe bağlı)", Width = 200, Location = new Point(25, yPos) };
                txtMinLabel.TextChanged += (_, _) => { MinLabel = txtMinLabel.Text; };
                
                panel.Controls.Add(lblMinText);
                panel.Controls.Add(txtMinLabel);
                yPos += 30;

                var lblMaxText = new Label { Text = cbMax.SelectedItem?.ToString(), Width = 20, Location = new Point(0, yPos + 4) };
                var txtMaxLabel = new TextBox { Text = MaxLabel, PlaceholderText = "Etiket (isteğe bağlı)", Width = 200, Location = new Point(25, yPos) };
                txtMaxLabel.TextChanged += (_, _) => { MaxLabel = txtMaxLabel.Text; };

                panel.Controls.Add(lblMaxText);
                panel.Controls.Add(txtMaxLabel);
                yPos += 40;
            }

            int count = (MaxValue - MinValue) + 1;
            int tlpWidth = 500;
            var tlp = new TableLayoutPanel
            {
                Height = 45,
                Width = tlpWidth,
                RowCount = 1,
                ColumnCount = count
            };
            for (int i = 0; i < count; i++)
            {
                tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / count));
            }

            for (int i = MinValue; i <= MaxValue; i++)
            {
                var rb = new RadioButton
                {
                    Text = i.ToString(),
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    Name = Id,
                    Enabled = !isDesignerMode,
                    CheckAlign = ContentAlignment.TopCenter,
                    TextAlign = ContentAlignment.BottomCenter,
                    Anchor = AnchorStyles.None,
                    Margin = new Padding(0)
                };
                tlp.Controls.Add(rb, i - MinValue, 0);
            }
            
            panel.Size = new Size(600, yPos + 80);
            tlp.Location = new Point(System.Math.Max(0, (panel.Width - tlpWidth) / 2), yPos);
            tlp.Anchor = AnchorStyles.Top;
            
            panel.Controls.Add(tlp);
            yPos += 50;

            if (!string.IsNullOrWhiteSpace(MinLabel) || !string.IsNullOrWhiteSpace(MaxLabel))
            {
                if (!string.IsNullOrWhiteSpace(MinLabel))
                {
                    var lMin = new Label { Text = MinLabel, AutoSize = true, ForeColor = Color.Gray, Location = new Point(tlp.Left, yPos) };
                    panel.Controls.Add(lMin);
                }
                if (!string.IsNullOrWhiteSpace(MaxLabel))
                {
                    var lMax = new Label { Text = MaxLabel, AutoSize = true, ForeColor = Color.Gray };
                    panel.Controls.Add(lMax);
                    lMax.Location = new Point(tlp.Right - lMax.Width, yPos);
                }
                yPos += 30;
            }

            yPos = AddFooterControls(panel, yPos, isSelected, onUpdate, onDelete);

            panel.Size = new Size(600, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var controls = renderedControl.Controls.Find(Id, true);
            foreach (var c in controls)
            {
                if (c is RadioButton rb && rb.Checked) return rb.Text;
            }
            return null;
        }
    }
}
