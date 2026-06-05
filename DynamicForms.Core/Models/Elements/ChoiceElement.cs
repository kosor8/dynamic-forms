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

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel);

            if (SubType == ChoiceType.DropDown)
            {
                var cb = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 250,
                    Location = new Point(0, yPos),
                    Enabled = !isDesignerMode,
                    Name = Id
                };
                cb.Items.AddRange(Options.ToArray());
                panel.Controls.Add(cb);
                yPos += 30;
            }
            else
            {
                foreach (var option in Options)
                {
                    Control ctrl;
                    if (SubType == ChoiceType.CheckBox)
                        ctrl = new CheckBox { Text = option, AutoSize = true, FlatStyle = FlatStyle.Flat };
                    else
                        ctrl = new RadioButton { Text = option, AutoSize = true, FlatStyle = FlatStyle.Flat };

                    ctrl.Location = new Point(0, yPos);
                    ctrl.Enabled = !isDesignerMode;
                    ctrl.Name = Id;
                    panel.Controls.Add(ctrl);
                    yPos += 26;
                }
            }

            panel.Size = new Size(400, yPos);
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
