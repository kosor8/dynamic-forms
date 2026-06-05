using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DynamicForms.Core.Database;
using DynamicForms.Core.Managers;
using DynamicForms.Core.Models;
using DynamicForms.Core.UI;

namespace DynamicForms.Filler
{
    public class FillerForm : Form
    {
        private DatabaseEngine _dbEngine;
        private List<FormElement> _currentElements;
        private string _currentFormId;

        // UI Panels
        private Panel topBar;
        private Panel pnlSelector;
        private FlowLayoutPanel pnlFiller;
        private Panel pnlSuccess;

        public FillerForm()
        {
            _dbEngine = new DatabaseEngine();
            _currentElements = new List<FormElement>();

            Text = "Dynamic Forms";
            Size = new Size(800, 750);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9);
            BackColor = ColorTranslator.FromHtml("#F5F5F5");

            InitializeLayout();
            ShowSelector();
        }

        private void InitializeLayout()
        {
            // Top Bar
            topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = ColorTranslator.FromHtml("#4A90D9") };
            Label lblLogo = new Label { Text = "Form Doldur", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(15, 12) };
            
            ModernButton btnBack = new ModernButton { Text = "← Ana Menü", BackColor = ColorTranslator.FromHtml("#5C6BC0"), Width = 110, Height = 30, Location = new Point(this.Width - 135, 10), Anchor = AnchorStyles.Top | AnchorStyles.Right, Visible = false };
            btnBack.Click += (s, e) => ShowSelector();
            
            topBar.Controls.Add(lblLogo);
            topBar.Controls.Add(btnBack);
            Controls.Add(topBar);

            // Selector Panel
            pnlSelector = new Panel { Dock = DockStyle.Fill, BackColor = ColorTranslator.FromHtml("#F5F5F5") };
            Controls.Add(pnlSelector);

            // Filler Panel
            pnlFiller = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = ColorTranslator.FromHtml("#F5F5F5"), AutoScroll = true, Padding = new Padding(30), FlowDirection = FlowDirection.TopDown, WrapContents = false };
            pnlFiller.SizeChanged += (s, e) => {
                foreach (Control c in pnlFiller.Controls) {
                    if (c is Panel cardPanel) cardPanel.Width = pnlFiller.ClientSize.Width - 60;
                }
            };
            Controls.Add(pnlFiller);

            // Success Panel
            pnlSuccess = new Panel { Dock = DockStyle.Fill, BackColor = ColorTranslator.FromHtml("#F5F5F5") };
            Controls.Add(pnlSuccess);
        }

        private void ShowSelector()
        {
            pnlFiller.Visible = false;
            pnlSuccess.Visible = false;
            pnlSelector.Visible = true;
            topBar.Controls[1].Visible = false; // Hide back button

            pnlSelector.Controls.Clear();
            var forms = _dbEngine.GetPublishedForms();

            if (forms.Count == 0)
            {
                Label lblEmpty = new Label { Text = "Henüz yayınlanmış bir form bulunmuyor.", Font = new Font("Segoe UI", 12), AutoSize = true, Location = new Point(50, 50) };
                pnlSelector.Controls.Add(lblEmpty);
                return;
            }

            Panel centerCard = new Panel { Width = 500, Height = 250, BackColor = Color.White, Location = new Point((Width - 500) / 2, 100), Anchor = AnchorStyles.Top };
            centerCard.Paint += (s, pe) => {
                using (Pen p = new Pen(Color.FromArgb(230, 230, 230))) pe.Graphics.DrawRectangle(p, 0, 0, centerCard.Width - 1, centerCard.Height - 1);
            };

            Label lblTitle = new Label { Text = "Doldurmak İstediğiniz Formu Seçin", Font = new Font("Segoe UI", 14, FontStyle.Bold), AutoSize = true, Location = new Point(30, 30) };
            ComboBox comboForms = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 440, Location = new Point(30, 80), Font = new Font("Segoe UI", 11) };
            comboForms.DataSource = new BindingSource(forms.ToList(), null);
            comboForms.DisplayMember = "Value";
            comboForms.ValueMember = "Key";

            ModernButton btnOpen = new ModernButton { Text = "Formu Aç", BackColor = ColorTranslator.FromHtml("#4A90D9"), Width = 440, Height = 40, Location = new Point(30, 150), Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            btnOpen.Click += (s, e) => {
                if (comboForms.SelectedValue != null)
                {
                    _currentFormId = comboForms.SelectedValue.ToString();
                    string title = ((KeyValuePair<string, string>)comboForms.SelectedItem).Value;
                    OpenForm(title);
                }
            };

            centerCard.Controls.Add(lblTitle);
            centerCard.Controls.Add(comboForms);
            centerCard.Controls.Add(btnOpen);
            pnlSelector.Controls.Add(centerCard);
        }

        private void OpenForm(string formTitle)
        {
            pnlSelector.Visible = false;
            pnlSuccess.Visible = false;
            pnlFiller.Visible = true;
            topBar.Controls[1].Visible = true; // Show back button
            pnlFiller.Controls.Clear();

            _currentElements = _dbEngine.LoadFormStructure(_currentFormId);
            _currentElements = _currentElements.OrderBy(x => x.OrderIndex).ToList();

            Label lblTitle = new Label { Text = formTitle, Font = new Font("Segoe UI", 20, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 10, 0, 10) };
            pnlFiller.Controls.Add(lblTitle);

            Panel divider = new Panel { Width = pnlFiller.ClientSize.Width - 60, Height = 2, BackColor = ColorTranslator.FromHtml("#4A90D9"), Margin = new Padding(0, 0, 0, 20) };
            pnlFiller.Controls.Add(divider);

            foreach (var element in _currentElements)
            {
                Control render = element.RenderControl(isDesignerMode: false);
                render.Width = pnlFiller.ClientSize.Width - 60;
                pnlFiller.Controls.Add(render);
            }

            ModernButton btnSubmit = new ModernButton { Text = "Gönder", BackColor = ColorTranslator.FromHtml("#66BB6A"), Width = 200, Height = 45, Margin = new Padding(0, 10, 0, 40), Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            btnSubmit.Click += BtnSubmit_Click;
            pnlFiller.Controls.Add(btnSubmit);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            var errors = ValidationProvider.Validate(_currentElements, pnlFiller);
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, errors), "Doğrulama Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Dictionary<string, object> answers = new Dictionary<string, object>();
            foreach (var element in _currentElements)
            {
                if (element.Type != ElementType.TextBlock)
                {
                    answers.Add(element.Id, element.GetValue(pnlFiller));
                }
            }

            _dbEngine.SaveFormSubmission(_currentFormId, answers);
            ShowSuccess();
        }

        private void ShowSuccess()
        {
            pnlFiller.Visible = false;
            pnlSelector.Visible = false;
            pnlSuccess.Visible = true;
            topBar.Controls[1].Visible = false; // Hide back button

            pnlSuccess.Controls.Clear();

            Panel centerCard = new Panel { Width = 500, Height = 250, BackColor = Color.White, Location = new Point((Width - 500) / 2, 150), Anchor = AnchorStyles.Top };
            centerCard.Paint += (s, pe) => {
                using (Pen p = new Pen(Color.FromArgb(230, 230, 230))) pe.Graphics.DrawRectangle(p, 0, 0, centerCard.Width - 1, centerCard.Height - 1);
                using (SolidBrush b = new SolidBrush(ColorTranslator.FromHtml("#66BB6A"))) pe.Graphics.FillRectangle(b, 0, 0, centerCard.Width, 5);
            };

            Label lblCheck = new Label { Text = "✔", Font = new Font("Segoe UI", 36, FontStyle.Bold), ForeColor = ColorTranslator.FromHtml("#66BB6A"), AutoSize = true, Location = new Point(220, 30) };
            Label lblTitle = new Label { Text = "Teşekkür Ederiz!", Font = new Font("Segoe UI", 18, FontStyle.Bold), AutoSize = true, Location = new Point(150, 90) };
            Label lblDesc = new Label { Text = "Yanıtınız başarıyla kaydedildi.", Font = new Font("Segoe UI", 12), ForeColor = Color.Gray, AutoSize = true, Location = new Point(140, 130) };
            
            ModernButton btnNew = new ModernButton { Text = "Yeni Form Doldur", BackColor = ColorTranslator.FromHtml("#4A90D9"), Width = 200, Height = 40, Location = new Point(150, 180), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnNew.Click += (s, e) => ShowSelector();

            centerCard.Controls.Add(lblCheck);
            centerCard.Controls.Add(lblTitle);
            centerCard.Controls.Add(lblDesc);
            centerCard.Controls.Add(btnNew);
            pnlSuccess.Controls.Add(centerCard);
        }
    }
}
