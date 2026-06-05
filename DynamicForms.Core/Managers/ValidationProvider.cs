using System.Collections.Generic;
using System.Windows.Forms;
using DynamicForms.Core.Models;

namespace DynamicForms.Core.Managers
{
    public static class ValidationProvider
    {
        public static List<string> Validate(List<FormElement> elements, Control containerControl)
        {
            List<string> errors = new List<string>();

            foreach (var element in elements)
            {
                if (element.IsRequired)
                {
                    object value = element.GetValue(containerControl);
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    {
                        errors.Add($"\"{element.Title}\" alanı zorunludur.");
                    }
                }
            }

            return errors;
        }
    }
}
