using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DynamicForms.Core.UI;

namespace DynamicForms.Designer
{
    public class ResponseDetailForm : Form
    {
        public ResponseDetailForm(string formTitle, string submissionId, string submittedAt, Dictionary<string, string> qaPairs)
        {
            Text = "Yanıt Detayı";
            Size = new Size(550, 700);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9);
            BackColor = ColorTranslator.FromHtml("#F5F5F5");

            // Top Header Panel
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = ColorTranslator.FromHtml("#4A90D9") };
            Label lblTitle = new Label {
                Text = formTitle,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            Label lblSubTitle = new Label {
                Text = $"Tarih: {submittedAt} | ID: {submissionId}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(20, 42),
                AutoSize = true
            };
            topPanel.Controls.Add(lblTitle);
            topPanel.Controls.Add(lblSubTitle);
            Controls.Add(topPanel);

            // Close Button Panel
            Panel bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            ModernButton btnClose = new ModernButton {
                Text = "Kapat",
                Width = 120,
                Height = 35,
                Location = new Point(400, 12)
            };
            btnClose.Click += (s, e) => Close();
            
            // Draw a subtle border top for bottom panel
            bottomPanel.Paint += (s, pe) => {
                using (Pen p = new Pen(Color.FromArgb(230, 230, 230)))
                    pe.Graphics.DrawLine(p, 0, 0, bottomPanel.Width, 0);
            };
            bottomPanel.Controls.Add(btnClose);
            Controls.Add(bottomPanel);

            // Content Flow Panel
            FlowLayoutPanel flowPanel = new FlowLayoutPanel {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            Controls.Add(flowPanel);

            // Add questions and answers
            foreach (var qa in qaPairs)
            {
                Panel card = new Panel {
                    BackColor = Color.White,
                    Width = 470,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 15),
                    Padding = new Padding(20)
                };
                card.Paint += (s, pe) => {
                    using (Pen p = new Pen(Color.FromArgb(230, 230, 230)))
                        pe.Graphics.DrawRectangle(p, 0, 0, card.Width - 1, card.Height - 1);
                    using (SolidBrush b = new SolidBrush(ColorTranslator.FromHtml("#4A90D9")))
                        pe.Graphics.FillRectangle(b, 0, 0, 4, card.Height);
                };

                Label lblQuestion = new Label {
                    Text = qa.Key,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Width = 430,
                    AutoSize = true,
                    Location = new Point(15, 15)
                };
                Label lblAnswer = new Label {
                    Text = string.IsNullOrEmpty(qa.Value) ? "(Boş bırakılmış)" : qa.Value,
                    Font = new Font("Segoe UI", 10, FontStyle.Regular),
                    ForeColor = string.IsNullOrEmpty(qa.Value) ? Color.Gray : Color.Black,
                    Width = 430,
                    AutoSize = true,
                    Location = new Point(15, 45)
                };

                card.Controls.Add(lblQuestion);
                card.Controls.Add(lblAnswer);
                flowPanel.Controls.Add(card);
            }

            // Bring to front so layout behaves
            topPanel.SendToBack();
            bottomPanel.BringToFront();
            flowPanel.BringToFront();
        }
    }
}
