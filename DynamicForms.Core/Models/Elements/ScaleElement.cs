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
                var lblMin = new Label { Text = "Min:", AutoSize = true, Location = new Point(0, yPos + 2) };
                var nMin = new NumericUpDown { Minimum = 0, Maximum = 1, Value = MinValue, Width = 50, Location = new Point(35, yPos) };
                nMin.ValueChanged += (_, _) => { MinValue = (int)nMin.Value; onUpdate?.Invoke(); };

                var lblMax = new Label { Text = "Max:", AutoSize = true, Location = new Point(100, yPos + 2) };
                var nMax = new NumericUpDown { Minimum = 2, Maximum = 10, Value = MaxValue, Width = 50, Location = new Point(140, yPos) };
                nMax.ValueChanged += (_, _) => { MaxValue = (int)nMax.Value; onUpdate?.Invoke(); };

                panel.Controls.Add(lblMin);
                panel.Controls.Add(nMin);
                panel.Controls.Add(lblMax);
                panel.Controls.Add(nMax);
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
            
            panel.Size = new Size(600, yPos + 60);
            tlp.Location = new Point(System.Math.Max(0, (panel.Width - tlpWidth) / 2), yPos);
            tlp.Anchor = AnchorStyles.Top;
            
            panel.Controls.Add(tlp);
            yPos += 50;

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
