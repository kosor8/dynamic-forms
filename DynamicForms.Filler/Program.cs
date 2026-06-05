using System;
using System.Windows.Forms;

namespace DynamicForms.Filler
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FillerForm());
        }    
    }
}