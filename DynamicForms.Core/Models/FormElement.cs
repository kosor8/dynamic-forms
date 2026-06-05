using System;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DynamicForms.Core.Models
{
    public enum ElementType
    {
        TextBlock,
        OpenEnded,
        Choice,
        Scale,
        Grid,
        Time,
        FileUpload
    }

    public class CardPanel : Panel
    {
        private readonly bool _isRequired;

        public CardPanel(bool isRequired)
        {
            _isRequired = isRequired;
            DoubleBuffered = true;
            BackColor = Color.White;
            BorderStyle = BorderStyle.None;
            Margin = new Padding(0, 0, 0, 12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var bg = new SolidBrush(Color.White))
                g.FillRectangle(bg, ClientRectangle);

            for (int i = 1; i <= 3; i++)
            {
                int alpha = Math.Max(25 - i * 7, 1);
                using var pen = new Pen(Color.FromArgb(alpha, 0, 0, 0));
                g.DrawRectangle(pen, i, i, Width - 1 - i * 2, Height - 1 - i * 2);
            }

            using (var pen = new Pen(Color.FromArgb(224, 224, 224)))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            var barColor = _isRequired ? Color.FromArgb(239, 83, 80) : Color.FromArgb(74, 144, 217);
            using (var brush = new SolidBrush(barColor))
                g.FillRectangle(brush, 0, 0, 4, Height);
        }
    }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(Elements.TextBlockElement), "TextBlock")]
    [JsonDerivedType(typeof(Elements.OpenEndedElement), "OpenEnded")]
    [JsonDerivedType(typeof(Elements.ChoiceElement), "Choice")]
    [JsonDerivedType(typeof(Elements.ScaleElement), "Scale")]
    [JsonDerivedType(typeof(Elements.GridElement), "Grid")]
    [JsonDerivedType(typeof(Elements.TimeElement), "Time")]
    [JsonDerivedType(typeof(Elements.FileUploadElement), "FileUpload")]
    [XmlInclude(typeof(Elements.TextBlockElement))]
    [XmlInclude(typeof(Elements.OpenEndedElement))]
    [XmlInclude(typeof(Elements.ChoiceElement))]
    [XmlInclude(typeof(Elements.ScaleElement))]
    [XmlInclude(typeof(Elements.GridElement))]
    [XmlInclude(typeof(Elements.TimeElement))]
    [XmlInclude(typeof(Elements.FileUploadElement))]
    public abstract class FormElement
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = "Yeni Soru";
        public string Description { get; set; } = "";
        public bool IsRequired { get; set; }
        public ElementType Type { get; set; }
        public int OrderIndex { get; set; }
        public string? OptionsPayload { get; set; }

        public abstract Control RenderControl(bool isDesignerMode);
        public abstract object? GetValue(Control renderedControl);

        public Control WrapInCard(Control content, bool isDesignerMode)
        {
            var card = new CardPanel(IsRequired);
            int topOffset = 15;

            if (isDesignerMode)
            {
                var handle = new Label
                {
                    Text = "\u2630",
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    ForeColor = Color.FromArgb(180, 180, 180),
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 2),
                    Size = new Size(800, 22),
                    Cursor = Cursors.SizeAll,
                    Name = "DragHandle"
                };
                card.Controls.Add(handle);
                topOffset = 28;
            }

            content.Location = new Point(20, topOffset);
            card.Controls.Add(content);
            card.Height = topOffset + content.Height + 20;

            return card;
        }

        protected int AddTitleAndDescription(Panel panel)
        {
            string titleText = IsRequired ? Title + " *" : Title;
            var lblTitle = new Label
            {
                Text = titleText,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 33, 33),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            panel.Controls.Add(lblTitle);
            int yPos = 28;

            if (!string.IsNullOrEmpty(Description))
            {
                var lblDesc = new Label
                {
                    Text = Description,
                    AutoSize = true,
                    ForeColor = Color.FromArgb(117, 117, 117),
                    Font = new Font("Segoe UI", 8.5f),
                    Location = new Point(0, yPos)
                };
                panel.Controls.Add(lblDesc);
                yPos += 22;
            }
            return yPos + 5;
        }
    }
}
