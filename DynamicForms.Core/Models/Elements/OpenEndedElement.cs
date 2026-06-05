using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public enum OpenEndedType { ShortAnswer, Paragraph }

    public class OpenEndedElement : FormElement
    {
        public OpenEndedType SubType { get; set; } = OpenEndedType.ShortAnswer;

        public OpenEndedElement()
        {
            Type = ElementType.OpenEnded;
            Title = "Açık Uçlu Soru";
        }

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel);

            Control input;
            if (SubType == OpenEndedType.Paragraph)
            {
                input = new TextBox
                {
                    Multiline = true,
                    Width = 350,
                    Height = 80,
                    Location = new Point(0, yPos),
                    Enabled = !isDesignerMode,
                    Name = Id
                };
                yPos += 85;
            }
            else
            {
                input = new TextBox
                {
                    Width = 300,
                    Height = 23,
                    Location = new Point(0, yPos),
                    Enabled = !isDesignerMode,
                    Name = Id
                };
                yPos += 28;
            }
            panel.Controls.Add(input);

            panel.Size = new Size(400, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var controls = renderedControl.Controls.Find(Id, true);
            if (controls.Length > 0 && controls[0] is TextBox txt)
                return txt.Text;
            return null;
        }
    }
}
