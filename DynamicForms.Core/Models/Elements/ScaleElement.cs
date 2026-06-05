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

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel);

            int count = (MaxValue - MinValue) + 1;
            int xPos = 0;
            for (int i = MinValue; i <= MaxValue; i++)
            {
                var rb = new RadioButton
                {
                    Text = i.ToString(),
                    AutoSize = true,
                    FlatStyle = FlatStyle.Flat,
                    Name = Id,
                    Enabled = !isDesignerMode,
                    Location = new Point(xPos, yPos)
                };
                panel.Controls.Add(rb);
                xPos += 55;
            }
            yPos += 30;

            panel.Size = new Size(400, yPos);
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
