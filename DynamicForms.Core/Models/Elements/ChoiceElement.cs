using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public enum ChoiceType { Radio, CheckBox, DropDown }

    public class ChoiceElement : FormElement
    {
        public ChoiceType SubType { get; set; } = ChoiceType.Radio;
        public List<string> Options { get; set; } = new List<string> { "Seçenek 1" };

        public ChoiceElement()
        {
            Type = ElementType.Choice;
            Title = "Seçmeli Soru";
        }

        public override Control RenderControl(bool isDesignerMode, bool isSelected = false, System.Action? onUpdate = null, System.Action? onDelete = null)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel, isSelected, onUpdate);

            if (isSelected)
            {
                var cbSubType = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 250,
                    Location = new Point(0, yPos)
                };
                cbSubType.Items.AddRange(System.Enum.GetNames(typeof(ChoiceType)));
                cbSubType.SelectedItem = SubType.ToString();
                cbSubType.SelectedIndexChanged += (_, _) => { SubType = System.Enum.Parse<ChoiceType>(cbSubType.SelectedItem?.ToString() ?? "Radio"); onUpdate?.Invoke(); };
                panel.Controls.Add(cbSubType);
                yPos += 40;
            }

            for (int i = 0; i < Options.Count; i++)
            {
                int idx = i;
                if (isSelected)
                {
                    var txtOpt = new TextBox
                    {
                        Text = Options[i],
                        Width = 300,
                        Location = new Point(25, yPos)
                    };
                    txtOpt.TextChanged += (_, _) => { Options[idx] = txtOpt.Text; };
                    panel.Controls.Add(txtOpt);

                    if (Options.Count > 1)
                    {
                        var btnRem = new Button
                        {
                            Text = "✖",
                            Size = new Size(25, 25),
                            Location = new Point(330, yPos - 1),
                            FlatStyle = FlatStyle.Flat,
                            ForeColor = Color.Gray,
                            Cursor = Cursors.Hand,
                            TextAlign = ContentAlignment.MiddleCenter,
                            Padding = new Padding(0)
                        };
                        btnRem.FlatAppearance.BorderSize = 0;
                        btnRem.Click += (_, _) => { Options.RemoveAt(idx); onUpdate?.Invoke(); };
                        panel.Controls.Add(btnRem);
                    }
                    
                    var lblIcon = new Label
                    {
                        Text = SubType == ChoiceType.Radio ? "⚪" : SubType == ChoiceType.CheckBox ? "☐" : "▼",
                        AutoSize = true,
                        Location = new Point(0, yPos + 3),
                        ForeColor = Color.Gray
                    };
                    panel.Controls.Add(lblIcon);
                    
                    yPos += 30;
                }
                else
                {
                    if (SubType == ChoiceType.DropDown)
                    {
                        if (i == 0)
                        {
                            var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250, Location = new Point(0, yPos), Enabled = !isDesignerMode, Name = Id };
                            cb.Items.AddRange(Options.ToArray());
                            panel.Controls.Add(cb);
                            yPos += 30;
                        }
                    }
                    else
                    {
                        Control ctrl;
                        if (SubType == ChoiceType.CheckBox)
                            ctrl = new CheckBox { Text = Options[i], AutoSize = true, FlatStyle = FlatStyle.Flat };
                        else
                            ctrl = new RadioButton { Text = Options[i], AutoSize = true, FlatStyle = FlatStyle.Flat };

                        ctrl.Location = new Point(0, yPos);
                        ctrl.Enabled = !isDesignerMode;
                        ctrl.Name = Id;
                        panel.Controls.Add(ctrl);
                        yPos += 26;
                    }
                }
            }

            if (isSelected)
            {
                var btnAdd = new Button
                {
                    Text = "Seçenek ekle",
                    AutoSize = true,
                    Location = new Point(25, yPos),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.DodgerBlue,
                    Cursor = Cursors.Hand
                };
                btnAdd.FlatAppearance.BorderSize = 0;
                btnAdd.Click += (_, _) => { Options.Add($"Seçenek {Options.Count + 1}"); onUpdate?.Invoke(); };
                panel.Controls.Add(btnAdd);
                yPos += 30;
            }

            yPos = AddFooterControls(panel, yPos, isSelected, onUpdate, onDelete);

            panel.Size = new Size(500, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var controls = renderedControl.Controls.Find(Id, true);

            if (SubType == ChoiceType.DropDown)
            {
                if (controls.Length > 0 && controls[0] is ComboBox cb)
                    return cb.SelectedItem?.ToString();
            }
            else
            {
                var selected = new List<string>();
                foreach (var c in controls)
                {
                    if (c is CheckBox chk && chk.Checked) selected.Add(chk.Text);
                    else if (c is RadioButton rb && rb.Checked) selected.Add(rb.Text);
                }
                return string.Join(", ", selected);
            }
            return null;
        }
    }
}
