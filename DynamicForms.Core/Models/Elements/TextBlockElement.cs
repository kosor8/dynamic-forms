using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public class TextBlockElement : FormElement
    {
        public TextBlockElement()
        {
            Type = ElementType.TextBlock;
            Title = "Başlık";
            Description = "Açıklama metni...";
        }

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            var lblTitle = new Label
            {
                Text = Title,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            panel.Controls.Add(lblTitle);
            int yPos = 35;

            var lblDesc = new Label
            {
                Text = Description,
                Font = new Font("Segoe UI", 10),
                AutoSize = true,
                Location = new Point(0, yPos)
            };
            panel.Controls.Add(lblDesc);
            yPos += 25;

            panel.Size = new Size(400, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            return null;
        }
    }
}
