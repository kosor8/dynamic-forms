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

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel);

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
