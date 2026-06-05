using System;
using System.Windows.Forms;

namespace DynamicForms.Designer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }    
    }
}