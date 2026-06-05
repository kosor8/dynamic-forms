using System.Drawing;
using System.Windows.Forms;

namespace DynamicForms.Core.Models.Elements
{
    public class FileUploadElement : FormElement
    {
        public FileUploadElement()
        {
            Type = ElementType.FileUpload;
            Title = "Dosya Yükleme";
        }

        public override Control RenderControl(bool isDesignerMode)
        {
            var panel = new Panel();
            int yPos = AddTitleAndDescription(panel);

            var txtPath = new TextBox
            {
                Width = 200,
                Location = new Point(0, yPos),
                ReadOnly = true,
                Name = Id
            };

            var btnUpload = new Button
            {
                Text = "Dosya Seç...",
                Location = new Point(210, yPos - 2),
                Enabled = !isDesignerMode,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(245, 245, 245),
                AutoSize = true
            };
            btnUpload.FlatAppearance.BorderSize = 1;
            btnUpload.FlatAppearance.BorderColor = Color.LightGray;
            btnUpload.Click += (s, e) =>
            {
                using var ofd = new OpenFileDialog();
                if (ofd.ShowDialog() == DialogResult.OK)
                    txtPath.Text = ofd.FileName;
            };

            panel.Controls.Add(txtPath);
            panel.Controls.Add(btnUpload);
            yPos += 35;

            panel.Size = new Size(400, yPos);
            return WrapInCard(panel, isDesignerMode);
        }

        public override object? GetValue(Control renderedControl)
        {
            var controls = renderedControl.Controls.Find(Id, true);
            if (controls.Length > 0 && controls[0] is TextBox txt)
                return txt.Text;
            return null;
        }
    }
}
