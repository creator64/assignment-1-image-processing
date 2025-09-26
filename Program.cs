using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace INFOIBV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0) startApplication();
            Tasks tasks = new Tasks();
            if (args[0] == "task1")
            {
                decimal sigma = decimal.Parse(args[1]);
                tasks.Task1("images/image_for_tasks_1_and_2.jpg", sigma);
            }
        }

        static void startApplication()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new INFOIBV());
        }
    }
}
