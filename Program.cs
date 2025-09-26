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
            int[,] filter = FilterGenerators.createSquareFilter<int>(13, FilterValueGenerators.createGaussianSquareFilter);
            for(int i = 0; i < filter.GetLength(0); i++)
            {
                Debug.Write("[\t");
                for(int j = 0;  j < filter.GetLength(1); j++)
                {
                    Debug.Write($"{filter[i, j]},\t");
                }
                Debug.Write("]\n");
            }
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
