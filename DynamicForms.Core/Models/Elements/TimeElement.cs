using System;
using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public enum TimeType { Date, Time }

    public class TimeElement : FormElement
    {
        public TimeType SubType { get; set; } = TimeType.Date;

        public TimeElement()
        {
            Type = ElementType.Time;
            Title = "Zaman Sorusu";
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
                cbSubType.Items.AddRange(System.Enum.GetNames(typeof(TimeType)));
                cbSubType.SelectedItem = SubType.ToString();
                cbSubType.SelectedIndexChanged += (_, _) => { SubType = System.Enum.Parse<TimeType>(cbSubType.SelectedItem?.ToString() ?? "Date"); onUpdate?.Invoke(); };
                panel.Controls.Add(cbSubType);
                yPos += 35;
            }

            var dtp = new DateTimePicker
            {
                Location = new Point(0, yPos),
                Width = 200,
                Name = Id,
                Enabled = !isDesignerMode
            };

            if (SubType == TimeType.Date)
                dtp.Format = DateTimePickerFormat.Short;
            else
            {
                dtp.Format = DateTimePickerFormat.Time;
                dtp.ShowUpDown = true;
            }

            panel.Controls.Add(dtp);
            yPos += 30;

            yPos = AddFooterControls(panel, yPos, isSelected, onUpdate, onDelete);

            panel.Size = new Size(400, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var controls = renderedControl.Controls.Find(Id, true);
            if (controls.Length > 0 && controls[0] is DateTimePicker dtp)
                return SubType == TimeType.Date ? dtp.Value.ToShortDateString() : dtp.Value.ToShortTimeString();
            return null;
        }
    }
}
