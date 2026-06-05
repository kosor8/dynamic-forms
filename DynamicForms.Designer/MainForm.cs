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
        private ListBox listForms = null!;
        private ComboBox comboResponsesForm = null!;
        private DataGridView gridResponses = null!;

        private int _dropIndex = -1;

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
            NewForm();
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

            // ── RIGHT PANEL REMOVED ──
            tab.Controls.Add(leftPanel);

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
            designerArea.DragLeave += DesignerArea_DragLeave;
            designerArea.Paint += DesignerArea_Paint;
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
            _formManager.FormTitle = "Yeni Form";
            _formManager.FormDescription = "";
            _selectedElement = null;
            RefreshDesigner();
        }

        private void LoadFormsList()
        {
            var forms = _dbEngine.GetSavedForms();
            listForms.DataSource = new BindingSource(forms.ToList(), "");
            listForms.DisplayMember = "Value";
            listForms.ValueMember = "Key";
        }

        private void LoadResponsesDropdown()
        {
            var forms = _dbEngine.GetSavedForms();
            comboResponsesForm.DataSource = new BindingSource(forms.ToList(), "");
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
                _formManager.FormDescription = ""; 
                _formManager.CurrentFormElements.Clear();
                foreach (var el in _dbEngine.LoadFormStructure(formId))
                    _formManager.AddElement(el);
                _selectedElement = null;
                RefreshDesigner();
            }
        }

        private void SaveForm()
        {
            _dbEngine.SaveFormStructure(_currentFormId, _formManager.FormTitle, _formManager.CurrentFormElements);
            MessageBox.Show("Form başarıyla kaydedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadFormsList();
        }

        private void PublishForm()
        {
            if (string.IsNullOrWhiteSpace(_formManager.FormTitle))
            {
                MessageBox.Show("Lütfen bir form başlığı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_formManager.CurrentFormElements.Count == 0)
            {
                MessageBox.Show("Yayınlamadan önce forma en az bir soru ekleyin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
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
            RefreshDesigner();
            RefreshDesigner();
        }

        private void SelectElement(FormElement el)
        {
            if (_selectedElement == el) return;
            _selectedElement = el;
            RefreshDesigner();
        }

        private void DeleteSelectedElement()
        {
            if (_selectedElement != null)
            {
                _formManager.RemoveElement(_selectedElement);
                _selectedElement = null;
                RefreshDesigner();
            }
        }

        // ──────────────────── DESIGNER RENDERING ────────────────────

        private void RefreshDesigner()
        {
            designerArea.SuspendLayout();
            designerArea.Controls.Clear();

            // ──────────────────── HEADER CARD ────────────────────
            var headerCard = new CardPanel(false) { Height = 130 };
            int hw = designerArea.ClientSize.Width - 55;
            headerCard.Width = hw < 300 ? 300 : hw;

            var topBorder = new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.FromArgb(103, 58, 183) }; // Google Forms style purple bar
            headerCard.Controls.Add(topBorder);

            var txtTitle = new TextBox 
            { 
                Text = _formManager.FormTitle, 
                Font = new Font("Segoe UI", 24f, FontStyle.Bold), 
                ForeColor = Color.FromArgb(33, 33, 33), 
                Width = headerCard.Width - 40, 
                BorderStyle = BorderStyle.None, 
                PlaceholderText = "Form Başlığı", 
                Location = new Point(20, 30) 
            };
            txtTitle.TextChanged += (_, _) => _formManager.FormTitle = txtTitle.Text;
            
            var txtDesc = new TextBox 
            { 
                Text = _formManager.FormDescription,
                Font = new Font("Segoe UI", 11f), 
                ForeColor = Color.FromArgb(117, 117, 117), 
                Width = headerCard.Width - 40, 
                BorderStyle = BorderStyle.None, 
                PlaceholderText = "Form açıklaması", 
                Location = new Point(20, 85) 
            };
            txtDesc.TextChanged += (_, _) => _formManager.FormDescription = txtDesc.Text;

            headerCard.Controls.Add(txtTitle);
            headerCard.Controls.Add(txtDesc);

            designerArea.Controls.Add(headerCard);
            // ────────────────────────────────────────────────────────

            foreach (var el in _formManager.CurrentFormElements.OrderBy(e => e.OrderIndex))
            {
                bool isSelected = _selectedElement != null && el.Id == _selectedElement.Id;
                Control card = el.RenderControl(
                    isDesignerMode: true,
                    isSelected: isSelected,
                    onUpdate: () => RefreshDesigner(),
                    onDelete: () => { _formManager.RemoveElement(el); _selectedElement = null; RefreshDesigner(); }
                );

                // Highlight selected
                if (_selectedElement != null && el.Id == _selectedElement.Id)
                    card.BackColor = Color.FromArgb(255, 253, 210); // Lighter yellow

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
                    
                    Point clientPt = designerArea.PointToClient(new Point(e.X, e.Y));
                    int targetIndex = designerArea.Controls.Count;
                    for (int i = 0; i < designerArea.Controls.Count; i++)
                    {
                        Control ctrl = designerArea.Controls[i];
                        if (clientPt.Y < ctrl.Top + ctrl.Height / 2)
                        {
                            targetIndex = i;
                            break;
                        }
                    }
                    if (targetIndex == 0 && designerArea.Controls.Count > 0) targetIndex = 1;

                    if (_dropIndex != targetIndex)
                    {
                        _dropIndex = targetIndex;
                        designerArea.Invalidate();
                    }
                    
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void DesignerArea_DragLeave(object? sender, EventArgs e)
        {
            Point pt = designerArea.PointToClient(Cursor.Position);
            if (!designerArea.ClientRectangle.Contains(pt))
            {
                if (_dropIndex != -1)
                {
                    _dropIndex = -1;
                    designerArea.Invalidate();
                }
            }
        }

        private void DesignerArea_Paint(object? sender, PaintEventArgs e)
        {
            if (_dropIndex >= 0 && _dropIndex <= designerArea.Controls.Count)
            {
                int yPos = 20;
                if (designerArea.Controls.Count > 0)
                {
                    if (_dropIndex < designerArea.Controls.Count)
                    {
                        yPos = designerArea.Controls[_dropIndex].Top - 6;
                    }
                    else
                    {
                        yPos = designerArea.Controls[designerArea.Controls.Count - 1].Bottom + 6;
                    }
                }

                using (var pen = new Pen(Color.DodgerBlue, 4))
                {
                    e.Graphics.DrawLine(pen, 20, yPos, designerArea.ClientSize.Width - 20, yPos);
                }
            }
        }

        private void DesignerArea_DragDrop(object? sender, DragEventArgs e)
        {
            _dropIndex = -1;
            designerArea.Invalidate();

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
            if (targetIndex == 0 && designerArea.Controls.Count > 0) targetIndex = 1;
            int insertIndex = Math.Max(0, targetIndex - 1);

            if (dataStr.StartsWith("ToolboxElement:"))
            {
                // Araç kutusundan yeni eleman sürüklenip bırakıldı
                string typeStr = dataStr.Substring("ToolboxElement:".Length);
                if (Enum.TryParse<ElementType>(typeStr, out ElementType type))
                {
                    var el = CreateElementByType(type);
                    
                    if (!_formManager.CurrentFormElements.Contains(el))
                    {
                        _formManager.CurrentFormElements.Add(el);
                    }

                    var currentList = _formManager.CurrentFormElements.OrderBy(x => x.OrderIndex).ToList();
                    currentList.Remove(el);
                    currentList.Insert(Math.Min(insertIndex, currentList.Count), el);
                    
                    for (int i = 0; i < currentList.Count; i++)
                    {
                        currentList[i].OrderIndex = i;
                    }

                    _selectedElement = el;
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
                        if (insertIndex > oldIdx) insertIndex--; // Kendisini çıkardığımız için hedef index kayabilir
                        currentList.Insert(Math.Min(insertIndex, currentList.Count), draggingEl);

                        for (int i = 0; i < currentList.Count; i++)
                        {
                            currentList[i].OrderIndex = i;
                        }

                        _selectedElement = draggingEl;
                        RefreshDesigner();
                    }
                }
            }
        }
    }
}
