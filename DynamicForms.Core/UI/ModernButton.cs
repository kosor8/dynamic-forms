using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DynamicForms.Core.UI
{
    public class ModernButton : Button
    {
        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = ColorTranslator.FromHtml("#4A90D9");
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9, FontStyle.Regular);
            Size = new Size(100, 35);
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateRegion();
        }

        private void UpdateRegion()
        {
            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int radius = 6;
            if (Width > radius && Height > radius)
            {
                using (GraphicsPath path = GetRoundPath(rect, radius))
                {
                    this.Region = new Region(path);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            // Do not call base to prevent default drawing
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width, Height);
            int radius = 6;

            using (GraphicsPath path = GetRoundPath(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(BackColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(
                    pevent.Graphics,
                    Text,
                    Font,
                    rect,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak
                );
            }
        }

        private GraphicsPath GetRoundPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            float r2 = radius * 2f;
            path.AddArc(rect.X, rect.Y, r2, r2, 180, 90);
            path.AddArc(rect.Right - r2 - 1, rect.Y, r2, r2, 270, 90);
            path.AddArc(rect.Right - r2 - 1, rect.Bottom - r2 - 1, r2, r2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - r2 - 1, r2, r2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
