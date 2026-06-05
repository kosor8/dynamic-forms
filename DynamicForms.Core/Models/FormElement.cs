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

            using (var bg = new SolidBrush(this.BackColor))
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

        public abstract Control RenderControl(bool isDesignerMode, bool isSelected = false, Action? onUpdate = null, Action? onDelete = null);
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
                    Dock = DockStyle.Top,
                    Height = 22,
                    Cursor = Cursors.SizeAll,
                    Name = "DragHandle"
                };
                card.Controls.Add(handle);
                topOffset = 28;
            }

            card.Width = content.Width + 40;
            content.Location = new Point(20, topOffset);
            content.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            card.Controls.Add(content);
            card.Height = topOffset + content.Height + 20;

            return card;
        }

        protected int AddTitleAndDescription(Panel panel, bool isSelected, Action? onUpdate)
        {
            int yPos = 0;
            string typeName = Type switch {
                ElementType.OpenEnded => "Açık Uçlu Soru",
                ElementType.Choice => "Çoktan Seçmeli Soru",
                ElementType.Scale => "Ölçek Sorusu",
                ElementType.Grid => "Tablo Seçim Sorusu",
                ElementType.Time => "Zaman Sorusu",
                ElementType.FileUpload => "Dosya Yükleme",
                _ => "Soru"
            };

            if (isSelected)
            {
                var txtTitle = new TextBox
                {
                    Text = (Title == typeName || Title == "Yeni Soru") ? "" : Title,
                    Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 33, 33),
                    Location = new Point(0, yPos),
                    Width = 500,
                    BorderStyle = BorderStyle.None,
                    PlaceholderText = typeName,
                    BackColor = panel.BackColor
                };
                txtTitle.TextChanged += (_, _) => { Title = string.IsNullOrWhiteSpace(txtTitle.Text) ? typeName : txtTitle.Text; };
                panel.Controls.Add(txtTitle);

                var titleLine = new Panel { BackColor = Color.LightGray, Height = 1, Width = 500, Location = new Point(0, yPos + 26) };
                panel.Controls.Add(titleLine);

                yPos += 35;
            }
            else
            {
                string displayTitle = string.IsNullOrWhiteSpace(Title) || Title == "Yeni Soru" ? typeName : Title;
                string titleText = IsRequired ? displayTitle + " *" : displayTitle;
                var lblTitle = new Label
                {
                    Text = titleText,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(33, 33, 33),
                    AutoSize = true,
                    Location = new Point(0, yPos)
                };
                panel.Controls.Add(lblTitle);
                yPos += 28;
            }

            return yPos + 5;
        }

        protected int AddFooterControls(Panel panel, int yPos, bool isSelected, Action? onUpdate, Action? onDelete, bool showRequired = true)
        {
            if (!isSelected) return yPos;

            var line = new Panel { BackColor = Color.LightGray, Height = 1, Width = 600, Location = new Point(0, yPos + 10) };
            panel.Controls.Add(line);
            yPos += 20;

            var btnDel = new Button
            {
                Text = "\U0001F5D1", // Çöp kutusu ikonu
                Font = new Font("Segoe UI", 12),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(40, 30),
                Location = new Point(400, yPos),
                Cursor = Cursors.Hand
            };
            btnDel.FlatAppearance.BorderSize = 0;
            btnDel.Click += (_, _) => onDelete?.Invoke();
            panel.Controls.Add(btnDel);

            if (showRequired)
            {
                var chkReq = new CheckBox
                {
                    Text = "Gerekli",
                    AutoSize = true,
                    Checked = IsRequired,
                    Location = new Point(470, yPos + 5)
                };
                chkReq.CheckedChanged += (_, _) => { IsRequired = chkReq.Checked; onUpdate?.Invoke(); };
                panel.Controls.Add(chkReq);
            }

            return yPos + 40;
        }
    }
}
