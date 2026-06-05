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

        public override Control RenderControl(bool isDesignerMode, bool isSelected = false, System.Action? onUpdate = null, System.Action? onDelete = null)
        {
            var panel = new Panel();
            int yPos = 0;

            if (isSelected)
            {
                var txtTitle = new TextBox
                {
                    Text = Title,
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    Width = 500,
                    Location = new Point(0, yPos),
                    BorderStyle = BorderStyle.None,
                    PlaceholderText = "Başlık"
                };
                txtTitle.TextChanged += (_, _) => { Title = txtTitle.Text; };
                panel.Controls.Add(txtTitle);
                
                var titleLine = new Panel { BackColor = Color.LightGray, Height = 1, Width = 500, Location = new Point(0, yPos + 32) };
                panel.Controls.Add(titleLine);
                yPos += 45;

                var txtDesc = new TextBox
                {
                    Text = Description,
                    Font = new Font("Segoe UI", 10),
                    Width = 500,
                    Location = new Point(0, yPos),
                    BorderStyle = BorderStyle.None,
                    PlaceholderText = "Açıklama"
                };
                txtDesc.TextChanged += (_, _) => { Description = txtDesc.Text; };
                panel.Controls.Add(txtDesc);

                var descLine = new Panel { BackColor = Color.LightGray, Height = 1, Width = 500, Location = new Point(0, yPos + 22) };
                panel.Controls.Add(descLine);
                yPos += 30;
            }
            else
            {
                var lblTitle = new Label
                {
                    Text = string.IsNullOrWhiteSpace(Title) ? "Başlık" : Title,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(0, yPos)
                };
                panel.Controls.Add(lblTitle);
                yPos += 35;

                var lblDesc = new Label
                {
                    Text = Description,
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    Location = new Point(0, yPos)
                };
                panel.Controls.Add(lblDesc);
                yPos += 25;
            }

            yPos = AddFooterControls(panel, yPos, isSelected, onUpdate, onDelete, showRequired: false);

            panel.Size = new Size(400, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            return null;
        }
    }
}
