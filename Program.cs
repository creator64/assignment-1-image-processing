using INFOIBV.Helper_Code;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Tasks tasks = new Tasks();

            if (args.Length == 0) startApplication();
            else
            {
                switch (args[0])
                {
                    case "task1":
                        decimal sigma = decimal.Parse(args[1]);
                        tasks.Task1("images/image_for_tasks_1_and_2.jpg", sigma);
                        break;
                    case "task2":
                        tasks.Task2("images/image_for_tasks_1_and_2.jpg");
                        break;
                    case "task3":
                        tasks.Task3("images/image_for_task3.jpg");
                        break;
                    default:
                        break;
                }
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
