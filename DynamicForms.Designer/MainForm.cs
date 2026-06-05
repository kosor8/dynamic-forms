using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DynamicForms.Core.Database;
using DynamicForms.Core.Managers;
using DynamicForms.Core.Models;
using DynamicForms.Core.Models.Elements;
using DynamicForms.Core.UI;

namespace DynamicForms.Designer
{
    public class MainForm : Form
    {
        private readonly FormManager _formManager;
        private readonly DatabaseEngine _dbEngine;
        private string _currentFormId = Guid.NewGuid().ToString();
        private FormElement? _selectedElement;

        // UI
        private TabControl tabControl = null!;
        private FlowLayoutPanel designerArea = null!;
        private Panel propertiesPanel = null!;
        private ListBox listForms = null!;
        private ComboBox comboResponsesForm = null!;
        private DataGridView gridResponses = null!;
        private TextBox txtFormTitle = null!;
        private TextBox txtPropTitle = null!;
        private TextBox txtPropDesc = null!;
        private CheckBox chkPropRequired = null!;
        private Panel panelDynamicProperties = null!;
        private bool _updatingProps;

        public MainForm()
        {
            _formManager = new FormManager();
            _dbEngine = new DatabaseEngine();

            Text = "Dynamic Forms Designer";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9);
            BackColor = Color.FromArgb(245, 245, 245);

            BuildUI();
            LoadFormsList();
        }

        // ─────────────────────────────────── UI BUILD ───────────────────────────────────
        private void BuildUI()
        {
            // ── TOP BAR ──
            var topBar = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.FromArgb(74, 144, 217) };
            topBar.Controls.Add(new Label
            {
                Text = "Dynamic Forms Designer",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            });

            var btnSave = new ModernButton
            {
                Text = "Kaydet",
                BackColor = Color.FromArgb(102, 187, 106),
                Size = new Size(100, 30),
                Location = new Point(Width - 250, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnSave.Click += (_, _) => SaveForm();
            topBar.Controls.Add(btnSave);

            var btnExport = new ModernButton
            {
                Text = "JSON İndir",
                BackColor = Color.FromArgb(92, 107, 192),
                Size = new Size(100, 30),
                Location = new Point(Width - 130, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnExport.Click += (_, _) => ExportJson();
            topBar.Controls.Add(btnExport);
            Controls.Add(topBar);

            // ── TAB CONTROL ──
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                ItemSize = new Size(150, 30),
                Font = new Font("Segoe UI", 10)
            };
            var tabDesign = new TabPage("Form Tasarımı");
            var tabResponses = new TabPage("Yanıtlar");
            tabControl.TabPages.Add(tabDesign);
            tabControl.TabPages.Add(tabResponses);
            tabControl.SelectedIndexChanged += (_, _) =>
            {
                if (tabControl.SelectedTab == tabResponses) LoadResponsesDropdown();
            };
            Controls.Add(tabControl);
            tabControl.BringToFront();

            // ════════════ TAB 1: DESIGN ════════════
            BuildDesignTab(tabDesign);

            // ════════════ TAB 2: RESPONSES ════════════
            BuildResponsesTab(tabResponses);
        }

        // ─── DESIGN TAB ───
        private void BuildDesignTab(TabPage tab)
        {
            // ── LEFT PANEL ──
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            leftPanel.Controls.Add(new Label
            {
                Text = "Kayıtlı Formlar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 10)
            });

            listForms = new ListBox
            {
                Location = new Point(10, 35),
                Size = new Size(210, 120),
                Font = new Font("Segoe UI", 9)
            };
            listForms.DoubleClick += (_, _) => LoadSelectedForm();
            leftPanel.Controls.Add(listForms);

            // Buttons row
            var btnNew = new ModernButton
            {
                Text = "+ Yeni Form",
                BackColor = Color.FromArgb(74, 144, 217),
                Size = new Size(95, 30),
                Location = new Point(10, 160)
            };
            btnNew.Click += (_, _) => NewForm();
            leftPanel.Controls.Add(btnNew);

            var btnDel = new ModernButton
            {
                Text = "Sil",
                BackColor = Color.FromArgb(239, 83, 80),
                Size = new Size(50, 30),
                Location = new Point(110, 160)
            };
            btnDel.Click += (_, _) => DeleteSelectedForm();
            leftPanel.Controls.Add(btnDel);

            var btnPub = new ModernButton
            {
                Text = "Yayınla",
                BackColor = Color.FromArgb(102, 187, 106),
                Size = new Size(55, 30),
                Location = new Point(165, 160)
            };
            btnPub.Click += (_, _) => PublishForm();
            leftPanel.Controls.Add(btnPub);

            // ── TOOLBOX ──
            leftPanel.Controls.Add(new Label
            {
                Text = "Araç Kutusu",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 205)
            });

            string[] toolNames = {
                "\U0001F4DD Açık Uçlu", "\U0001F518 Çoktan Seçmeli", "\U0001F4CF Ölçek",
                "\U0001F4CA Tablo Seçim", "\U0001F4C5 Tarih/Saat",
                "\U0001F4CE Dosya Yükleme", "Aa Metin Bloğu"
            };
            ElementType[] toolTypes = {
                ElementType.OpenEnded, ElementType.Choice, ElementType.Scale,
                ElementType.Grid, ElementType.Time,
                ElementType.FileUpload, ElementType.TextBlock
            };

            for (int i = 0; i < toolNames.Length; i++)
            {
                var idx = i;
                var btn = new Button
                {
                    Text = "  " + toolNames[i],
                    Size = new Size(210, 38),
                    Location = new Point(10, 230 + i * 42),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 9)
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = Color.FromArgb(224, 224, 224);
                btn.MouseEnter += (_, _) => btn.BackColor = Color.FromArgb(227, 242, 253);
                btn.MouseLeave += (_, _) => btn.BackColor = Color.White;
                btn.Click += (_, _) => AddElementByType(toolTypes[idx]);
                btn.MouseDown += (s, me) =>
                {
                    if (me.Button == MouseButtons.Left)
                    {
                        btn.DoDragDrop("ToolboxElement:" + toolTypes[idx].ToString(), DragDropEffects.Copy);
                    }
                };
                leftPanel.Controls.Add(btn);
            }
            tab.Controls.Add(leftPanel);

            // ── RIGHT PANEL (Properties) ──
            propertiesPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            propertiesPanel.Controls.Add(new Label
            {
                Text = "Özellikler",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 10)
            });

            propertiesPanel.Controls.Add(new Label { Text = "Form Başlığı:", AutoSize = true, Location = new Point(15, 40) });
            txtFormTitle = new TextBox { Width = 245, Location = new Point(15, 60), Text = _formManager.FormTitle };
            txtFormTitle.TextChanged += (_, _) => _formManager.FormTitle = txtFormTitle.Text;
            propertiesPanel.Controls.Add(txtFormTitle);

            propertiesPanel.Controls.Add(new Panel { Width = 245, Height = 1, BackColor = Color.LightGray, Location = new Point(15, 95) });
            propertiesPanel.Controls.Add(new Label
            {
                Text = "Seçili Soru Ayarları:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(15, 110)
            });

            propertiesPanel.Controls.Add(new Label { Text = "Soru Başlığı:", AutoSize = true, Location = new Point(15, 135) });
            txtPropTitle = new TextBox { Width = 245, Location = new Point(15, 155) };
            txtPropTitle.TextChanged += PropChanged;
            propertiesPanel.Controls.Add(txtPropTitle);

            propertiesPanel.Controls.Add(new Label { Text = "Açıklama:", AutoSize = true, Location = new Point(15, 185) });
            txtPropDesc = new TextBox { Width = 245, Location = new Point(15, 205) };
            txtPropDesc.TextChanged += PropChanged;
            propertiesPanel.Controls.Add(txtPropDesc);

            chkPropRequired = new CheckBox { Text = "Zorunlu Alan", AutoSize = true, Location = new Point(15, 235) };
            chkPropRequired.CheckedChanged += PropChanged;
            propertiesPanel.Controls.Add(chkPropRequired);

            var btnDelElem = new ModernButton
            {
                Text = "Soruyu Sil",
                BackColor = Color.FromArgb(239, 83, 80),
                Size = new Size(245, 30),
                Location = new Point(15, 265)
            };
            btnDelElem.Click += (_, _) => DeleteSelectedElement();
            propertiesPanel.Controls.Add(btnDelElem);

            panelDynamicProperties = new Panel
            {
                Location = new Point(15, 310),
                Size = new Size(245, 380),
                AutoScroll = true
            };
            propertiesPanel.Controls.Add(panelDynamicProperties);
            tab.Controls.Add(propertiesPanel);

            // ── DESIGNER AREA (center) ──
            designerArea = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 245, 245),
                AutoScroll = true,
                Padding = new Padding(20),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AllowDrop = true
            };
            designerArea.SizeChanged += (_, _) => UpdateCardWidths();
            designerArea.DragEnter += DesignerArea_DragEnter;
            designerArea.DragOver += DesignerArea_DragOver;
            designerArea.DragDrop += DesignerArea_DragDrop;
            tab.Controls.Add(designerArea);
            designerArea.BringToFront();
        }

        // ─── RESPONSES TAB ───
        private void BuildResponsesTab(TabPage tab)
        {
            var topPanel = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White, Padding = new Padding(15) };
            topPanel.Controls.Add(new Label { Text = "Form Seç:", AutoSize = true, Location = new Point(15, 20) });

            comboResponsesForm = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 300,
                Location = new Point(80, 17)
            };
            topPanel.Controls.Add(comboResponsesForm);

            var btnLoad = new ModernButton
            {
                Text = "Yanıtları Yükle",
                Size = new Size(120, 28),
                Location = new Point(390, 16)
            };
            btnLoad.Click += (_, _) => LoadResponses();
            topPanel.Controls.Add(btnLoad);

            var btnDetail = new ModernButton
            {
                Text = "Detayları Göster",
                BackColor = Color.FromArgb(92, 107, 192),
                Size = new Size(130, 28),
                Location = new Point(520, 16)
            };
            btnDetail.Click += (_, _) => ShowResponseDetail();
            topPanel.Controls.Add(btnDetail);
            tab.Controls.Add(topPanel);

            gridResponses = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            };
            gridResponses.DoubleClick += (_, _) => ShowResponseDetail();
            tab.Controls.Add(gridResponses);
            gridResponses.BringToFront();
        }

        // ─────────────────────────────────── ACTIONS ───────────────────────────────────

        private void NewForm()
        {
            _currentFormId = Guid.NewGuid().ToString();
            _formManager.ClearForm();
            txtFormTitle.Text = "Yeni Form";
            _selectedElement = null;
            UpdatePropertiesPanel();
            RefreshDesigner();
        }

        private void LoadFormsList()
        {
            var forms = _dbEngine.GetSavedForms();
            listForms.DataSource = new BindingSource(forms.ToList(), null);
            listForms.DisplayMember = "Value";
            listForms.ValueMember = "Key";
        }

        private void LoadResponsesDropdown()
        {
            var forms = _dbEngine.GetSavedForms();
            comboResponsesForm.DataSource = new BindingSource(forms.ToList(), null);
            comboResponsesForm.DisplayMember = "Value";
            comboResponsesForm.ValueMember = "Key";
        }

        private void LoadSelectedForm()
        {
            if (listForms.SelectedValue is string formId)
            {
                _currentFormId = formId;
                var formName = ((KeyValuePair<string, string>)listForms.SelectedItem!).Value;
                _formManager.FormTitle = formName;
                txtFormTitle.Text = formName;
                _formManager.CurrentFormElements.Clear();
                foreach (var el in _dbEngine.LoadFormStructure(formId))
                    _formManager.AddElement(el);
                _selectedElement = null;
                UpdatePropertiesPanel();
                RefreshDesigner();
            }
        }

        private void SaveForm()
        {
            _formManager.FormTitle = txtFormTitle.Text;
            _dbEngine.SaveFormStructure(_currentFormId, _formManager.FormTitle, _formManager.CurrentFormElements);
            MessageBox.Show("Form başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadFormsList();
        }

        private void PublishForm()
        {
            if (_formManager.CurrentFormElements.Count == 0)
            {
                MessageBox.Show("Yayınlamadan önce forma en az bir soru ekleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _formManager.FormTitle = txtFormTitle.Text;
            _dbEngine.SaveFormStructure(_currentFormId, _formManager.FormTitle, _formManager.CurrentFormElements);
            _dbEngine.PublishForm(_currentFormId);
            MessageBox.Show("Form kaydedildi ve yayınlandı!\nArtık Filler uygulamasından doldurulabilir.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadFormsList();
        }

        private void DeleteSelectedForm()
        {
            if (listForms.SelectedValue is string id)
            {
                if (MessageBox.Show("Bu formu ve tüm yanıtlarını silmek istediğinize emin misiniz?",
                    "Onay", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _dbEngine.DeleteForm(id);
                    if (_currentFormId == id) { _formManager.ClearForm(); RefreshDesigner(); }
                    LoadFormsList();
                }
            }
        }

        private void ExportJson()
        {
            using var sfd = new SaveFileDialog { Filter = "JSON Files|*.json" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                _formManager.ExportToJson(sfd.FileName);
                MessageBox.Show("JSON başarıyla dışa aktarıldı.");
            }
        }

        // ──────────────────── ADD / SELECT / DELETE ELEMENTS ────────────────────

        private FormElement CreateElementByType(ElementType type) => type switch
        {
            ElementType.TextBlock => new TextBlockElement(),
            ElementType.OpenEnded => new OpenEndedElement(),
            ElementType.Choice => new ChoiceElement(),
            ElementType.Scale => new ScaleElement(),
            ElementType.Grid => new GridElement(),
            ElementType.Time => new TimeElement(),
            ElementType.FileUpload => new FileUploadElement(),
            _ => new TextBlockElement()
        };

        private void AddElementByType(ElementType type)
        {
            var el = CreateElementByType(type);
            _formManager.AddElement(el);
            _selectedElement = el;
            UpdatePropertiesPanel();
            RefreshDesigner();
        }

        private void SelectElement(FormElement el)
        {
            _selectedElement = el;
            UpdatePropertiesPanel();
            RefreshDesigner();
        }

        private void DeleteSelectedElement()
        {
            if (_selectedElement != null)
            {
                _formManager.RemoveElement(_selectedElement);
                _selectedElement = null;
                UpdatePropertiesPanel();
                RefreshDesigner();
            }
        }

        // ──────────────────── DESIGNER RENDERING ────────────────────

        private void RefreshDesigner()
        {
            designerArea.SuspendLayout();
            designerArea.Controls.Clear();

            foreach (var el in _formManager.CurrentFormElements.OrderBy(e => e.OrderIndex))
            {
                Control card = el.RenderControl(isDesignerMode: true);

                // Highlight selected
                if (_selectedElement != null && el.Id == _selectedElement.Id)
                    card.BackColor = Color.FromArgb(255, 253, 231);

                // Set width
                int w = designerArea.ClientSize.Width - 55;
                if (w < 300) w = 300;
                card.Width = w;

                // Click to select
                AttachClick(card, el);

                // Drag handle for reorder
                var handles = card.Controls.Find("DragHandle", true);
                if (handles.Length > 0)
                {
                    handles[0].MouseDown += (s, me) =>
                    {
                        if (me.Button == MouseButtons.Left)
                            card.DoDragDrop(el.Id, DragDropEffects.Move);
                    };
                }

                designerArea.Controls.Add(card);
            }
            designerArea.ResumeLayout(true);
        }

        private void UpdateCardWidths()
        {
            int w = designerArea.ClientSize.Width - 55;
            if (w < 300) w = 300;
            foreach (Control c in designerArea.Controls) c.Width = w;
        }

        private void AttachClick(Control ctrl, FormElement el)
        {
            ctrl.Click += (_, _) => SelectElement(el);
            foreach (Control child in ctrl.Controls)
            {
                if (child.Name != "DragHandle")
                    AttachClick(child, el);
            }
        }

        // ──────────────────── PROPERTIES PANEL ────────────────────

        private void UpdatePropertiesPanel()
        {
            _updatingProps = true;
            panelDynamicProperties.Controls.Clear();

            if (_selectedElement == null)
            {
                txtPropTitle.Text = "";
                txtPropDesc.Text = "";
                chkPropRequired.Checked = false;
                _updatingProps = false;
                return;
            }

            txtPropTitle.Text = _selectedElement.Title;
            txtPropDesc.Text = _selectedElement.Description;
            chkPropRequired.Checked = _selectedElement.IsRequired;
            BuildDynamicProps(_selectedElement);
            _updatingProps = false;
        }

        private void PropChanged(object? sender, EventArgs e)
        {
            if (_updatingProps || _selectedElement == null) return;
            _selectedElement.Title = txtPropTitle.Text;
            _selectedElement.Description = txtPropDesc.Text;
            _selectedElement.IsRequired = chkPropRequired.Checked;
            RefreshDesigner();
        }

        private void BuildDynamicProps(FormElement el)
        {
            int y = 0;

            // Sub-type combo
            ComboBox? cbSub = null;
            if (el is OpenEndedElement oe)
            {
                cbSub = MakeSubTypeCombo<OpenEndedType>(oe.SubType.ToString(), y, v => { oe.SubType = Enum.Parse<OpenEndedType>(v); RefreshDesigner(); });
            }
            else if (el is ChoiceElement ce)
            {
                cbSub = MakeSubTypeCombo<ChoiceType>(ce.SubType.ToString(), y, v => { ce.SubType = Enum.Parse<ChoiceType>(v); RefreshDesigner(); });
            }
            else if (el is GridElement ge)
            {
                cbSub = MakeSubTypeCombo<GridType>(ge.SubType.ToString(), y, v => { ge.SubType = Enum.Parse<GridType>(v); RefreshDesigner(); });
            }
            else if (el is TimeElement te)
            {
                cbSub = MakeSubTypeCombo<TimeType>(te.SubType.ToString(), y, v => { te.SubType = Enum.Parse<TimeType>(v); RefreshDesigner(); });
            }

            if (cbSub != null)
            {
                panelDynamicProperties.Controls.Add(new Label { Text = "Alt Tür:", AutoSize = true, Location = new Point(0, y) });
                y += 20;
                cbSub.Location = new Point(0, y);
                panelDynamicProperties.Controls.Add(cbSub);
                y += 35;
            }

            // Choice options editor
            if (el is ChoiceElement choiceEl)
            {
                panelDynamicProperties.Controls.Add(new Label { Text = "Seçenekler:", AutoSize = true, Location = new Point(0, y) });
                y += 20;
                var txtOpt = new TextBox { Width = 170, Location = new Point(0, y) };
                var btnAdd = new Button { Text = "Ekle", Width = 55, Height = 23, Location = new Point(175, y) };
                y += 28;
                var lb = new ListBox { Width = 230, Height = 90, Location = new Point(0, y) };
                lb.Items.AddRange(choiceEl.Options.ToArray());
                y += 95;
                var btnRem = new Button { Text = "Seçiliyi Sil", Width = 230, Height = 25, Location = new Point(0, y) };

                btnAdd.Click += (_, _) =>
                {
                    if (!string.IsNullOrWhiteSpace(txtOpt.Text))
                    {
                        choiceEl.Options.Add(txtOpt.Text);
                        lb.Items.Add(txtOpt.Text);
                        txtOpt.Clear();
                        RefreshDesigner();
                    }
                };
                btnRem.Click += (_, _) =>
                {
                    if (lb.SelectedIndex >= 0)
                    {
                        choiceEl.Options.RemoveAt(lb.SelectedIndex);
                        lb.Items.RemoveAt(lb.SelectedIndex);
                        RefreshDesigner();
                    }
                };

                panelDynamicProperties.Controls.AddRange(new Control[] { txtOpt, btnAdd, lb, btnRem });
            }

            // Scale min/max
            if (el is ScaleElement scaleEl)
            {
                panelDynamicProperties.Controls.Add(new Label { Text = "Min - Max:", AutoSize = true, Location = new Point(0, y) });
                y += 20;
                var nMin = new NumericUpDown { Minimum = 0, Maximum = 1, Value = scaleEl.MinValue, Width = 50, Location = new Point(0, y) };
                var nMax = new NumericUpDown { Minimum = 2, Maximum = 10, Value = scaleEl.MaxValue, Width = 50, Location = new Point(80, y) };
                nMin.ValueChanged += (_, _) => { scaleEl.MinValue = (int)nMin.Value; RefreshDesigner(); };
                nMax.ValueChanged += (_, _) => { scaleEl.MaxValue = (int)nMax.Value; RefreshDesigner(); };
                panelDynamicProperties.Controls.Add(nMin);
                panelDynamicProperties.Controls.Add(nMax);
            }

            // Grid rows/cols
            if (el is GridElement gridEl)
            {
                panelDynamicProperties.Controls.Add(new Label { Text = "Satırlar (virgülle):", AutoSize = true, Location = new Point(0, y) });
                y += 20;
                var txtRows = new TextBox { Width = 230, Location = new Point(0, y), Text = string.Join(",", gridEl.Rows) };
                txtRows.Leave += (_, _) => { gridEl.Rows = txtRows.Text.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); RefreshDesigner(); };
                panelDynamicProperties.Controls.Add(txtRows);
                y += 28;
                panelDynamicProperties.Controls.Add(new Label { Text = "Sütunlar (virgülle):", AutoSize = true, Location = new Point(0, y) });
                y += 20;
                var txtCols = new TextBox { Width = 230, Location = new Point(0, y), Text = string.Join(",", gridEl.Columns) };
                txtCols.Leave += (_, _) => { gridEl.Columns = txtCols.Text.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList(); RefreshDesigner(); };
                panelDynamicProperties.Controls.Add(txtCols);
            }
        }

        private ComboBox MakeSubTypeCombo<T>(string currentValue, int y, Action<string> onChange) where T : struct, Enum
        {
            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
            cb.Items.AddRange(Enum.GetNames<T>());
            cb.SelectedItem = currentValue;
            cb.SelectedIndexChanged += (_, _) =>
            {
                if (cb.SelectedItem is string val) onChange(val);
            };
            return cb;
        }

        // ──────────────────── RESPONSES TAB ────────────────────

        private void LoadResponses()
        {
            if (comboResponsesForm.SelectedValue is string formId)
            {
                gridResponses.DataSource = _dbEngine.GetSubmissions(formId);
                if (gridResponses.Columns.Contains("SubmissionId"))
                    gridResponses.Columns["SubmissionId"]!.Visible = false;
            }
        }

        private void ShowResponseDetail()
        {
            if (gridResponses.SelectedRows.Count > 0)
            {
                var row = gridResponses.SelectedRows[0];
                string subId = row.Cells["SubmissionId"].Value?.ToString() ?? "";
                string date = row.Cells["Gönderim Tarihi"].Value?.ToString() ?? "";
                string formTitle = ((KeyValuePair<string, string>)comboResponsesForm.SelectedItem!).Value;
                var details = _dbEngine.GetSubmissionDetails(subId);
                new ResponseDetailForm(formTitle, subId, date, details).ShowDialog();
            }
        }

        // ──────────────────── DRAG & DROP HANDLERS ────────────────────

        private void DesignerArea_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.Text))
            {
                string? dataStr = e.Data.GetData(DataFormats.Text) as string;
                if (dataStr != null && (dataStr.StartsWith("ToolboxElement:") || _formManager.CurrentFormElements.Any(el => el.Id == dataStr)))
                {
                    e.Effect = dataStr.StartsWith("ToolboxElement:") ? DragDropEffects.Copy : DragDropEffects.Move;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void DesignerArea_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.Text))
            {
                string? dataStr = e.Data.GetData(DataFormats.Text) as string;
                if (dataStr != null && (dataStr.StartsWith("ToolboxElement:") || _formManager.CurrentFormElements.Any(el => el.Id == dataStr)))
                {
                    e.Effect = dataStr.StartsWith("ToolboxElement:") ? DragDropEffects.Copy : DragDropEffects.Move;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void DesignerArea_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.Text)) return;
            string? dataStr = e.Data.GetData(DataFormats.Text) as string;
            if (string.IsNullOrEmpty(dataStr)) return;

            Point clientPt = designerArea.PointToClient(new Point(e.X, e.Y));

            // Hangi sıraya yerleştireceğimizi belirlemek için fare pozisyonundaki kontrolü bulalım
            int targetIndex = _formManager.CurrentFormElements.Count;
            for (int i = 0; i < designerArea.Controls.Count; i++)
            {
                Control ctrl = designerArea.Controls[i];
                if (clientPt.Y < ctrl.Top + ctrl.Height / 2)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (dataStr.StartsWith("ToolboxElement:"))
            {
                // Araç kutusundan yeni eleman sürüklenip bırakıldı
                string typeStr = dataStr.Substring("ToolboxElement:".Length);
                if (Enum.TryParse<ElementType>(typeStr, out ElementType type))
                {
                    var el = CreateElementByType(type);
                    
                    // Elemanları yeniden sıralayalım
                    var currentList = _formManager.CurrentFormElements.OrderBy(x => x.OrderIndex).ToList();
                    currentList.Insert(Math.Min(targetIndex, currentList.Count), el);
                    
                    for (int i = 0; i < currentList.Count; i++)
                    {
                        currentList[i].OrderIndex = i;
                    }

                    if (!_formManager.CurrentFormElements.Contains(el))
                    {
                        _formManager.AddElement(el);
                    }

                    _selectedElement = el;
                    UpdatePropertiesPanel();
                    RefreshDesigner();
                }
            }
            else
            {
                // Mevcut eleman DragHandle yardımıyla sıralanıyor
                string elementId = dataStr;
                var draggingEl = _formManager.CurrentFormElements.FirstOrDefault(el => el.Id == elementId);
                if (draggingEl != null)
                {
                    var currentList = _formManager.CurrentFormElements.OrderBy(x => x.OrderIndex).ToList();
                    int oldIdx = currentList.IndexOf(draggingEl);
                    if (oldIdx != -1)
                    {
                        currentList.RemoveAt(oldIdx);
                        if (targetIndex > oldIdx) targetIndex--; // Kendisini çıkardığımız için hedef index kayabilir
                        currentList.Insert(Math.Min(targetIndex, currentList.Count), draggingEl);

                        for (int i = 0; i < currentList.Count; i++)
                        {
                            currentList[i].OrderIndex = i;
                        }

                        _selectedElement = draggingEl;
                        UpdatePropertiesPanel();
                        RefreshDesigner();
                    }
                }
            }
        }
    }
}
