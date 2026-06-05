using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using DynamicForms.Core.Models;

namespace DynamicForms.Core.Managers
{
    public class FormManager
    {
        public List<FormElement> CurrentFormElements { get; private set; } = new List<FormElement>();
        public string FormTitle { get; set; } = "Yeni Form";

        public void AddElement(FormElement element)
        {
            element.OrderIndex = CurrentFormElements.Count;
            CurrentFormElements.Add(element);
        }

        public void RemoveElement(FormElement element)
        {
            CurrentFormElements.Remove(element);
            ReorderElements();
        }

        public void MoveElementUp(FormElement element)
        {
            int index = CurrentFormElements.IndexOf(element);
            if (index > 0)
            {
                var temp = CurrentFormElements[index - 1];
                CurrentFormElements[index - 1] = element;
                CurrentFormElements[index] = temp;
                ReorderElements();
            }
        }

        public void MoveElementDown(FormElement element)
        {
            int index = CurrentFormElements.IndexOf(element);
            if (index < CurrentFormElements.Count - 1 && index >= 0)
            {
                var temp = CurrentFormElements[index + 1];
                CurrentFormElements[index + 1] = element;
                CurrentFormElements[index] = temp;
                ReorderElements();
            }
        }

        public void ReorderElements()
        {
            for (int i = 0; i < CurrentFormElements.Count; i++)
            {
                CurrentFormElements[i].OrderIndex = i;
            }
        }

        public void ClearForm()
        {
            CurrentFormElements.Clear();
            FormTitle = "Yeni Form";
        }

        public void ExportToJson(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(CurrentFormElements, options);
            File.WriteAllText(filePath, json);
        }

        public void ExportToXml(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<FormElement>));
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fs, CurrentFormElements);
            }
        }
    }
}
