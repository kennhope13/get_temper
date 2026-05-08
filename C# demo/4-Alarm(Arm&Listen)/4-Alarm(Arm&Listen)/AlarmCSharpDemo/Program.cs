using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AlarmCSharpDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new AlarmDemo());
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText(
                    @"C:\SdkLog\startup_error.txt",
                    ex.ToString()
                );
                MessageBox.Show(ex.ToString());
            };
        }
    }
}
