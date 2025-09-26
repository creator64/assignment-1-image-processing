using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using INFOIBV.Core;
using INFOIBV.Helper_Code;
using System.Text.Json;
using System.Diagnostics;
using System.Numerics;

namespace INFOIBV
{
    public class Tasks
    {
        public void Task1(string path, decimal sigma)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = enviroment;

            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = HelperFunctions.convertBitmapToColor(InputImage);
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);

            int[] sizes = { 3, 7, 11, 15, 19 };
            float[,] horizontalKernel = {
                { -1, -2, -1},
                { 0, 0, 0},
                { 1, 2, 1}
            };

            float[,] verticalKernel = {
                { -1, 0, 1},
                { -2, 0, 2},
                { -1, 0, 1}
            };

            for (int i = 0; i < sizes.Length; i++)
            {
                byte[,] processedImage = Pipelines.GaussianFilterAndEdgeDetection(sigma, sizes[i], horizontalKernel,
                    verticalKernel, workingImage);
                string outputPath = Path.Combine(basePath, "out", "task1", "sigma=" + sigma);

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                Image outputImage = HelperFunctions.convertToImage(processedImage);
                outputImage.Save(Path.Combine(outputPath, "B" + (i + 1) + ".bmp"), ImageFormat.Bmp);
            }
        }

        public void Task2(string path)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = enviroment;

            Debug.WriteLine(Path.Combine(basePath, path));
            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = HelperFunctions.convertBitmapToColor(InputImage);
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);

            int[] sizes = { 3, 7, 11, 15, 19 };

            for (int i = 0; i < sizes.Length; i++)
            {
                int[,] structElem = FilterGenerators.createSquareFilter<int>(sizes[i], FilterValueGenerators.createGaussianSquareFilter);
                byte[,] processedImage = ProcessingFunctions.grayscaleDilateImage(workingImage, structElem);

                string imgPath = Path.Combine(basePath, "out", "task2", "images");
                string dataPath = Path.Combine(basePath, "out", "task2", "data");

                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                ImageData data = new ImageData(processedImage);
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_C" + (i + 1) + ".json"), jsonString);

                Image outputImage = HelperFunctions.convertToImage(processedImage);

                outputImage.Save(Path.Combine(imgPath, "C" + (i + 1) + ".bmp"), ImageFormat.Bmp);

            }
        }
        public void Task3(string path)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = enviroment;

            Debug.WriteLine(Path.Combine(basePath, path));
            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = HelperFunctions.convertBitmapToColor(InputImage);
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);

            int[] sizes = { 3, 7, 11, 15, 19 };

            for (int i = 0; i < sizes.Length; i++)
            {
                int[,] structElem = FilterGenerators.createSquareFilter<int>(sizes[i], FilterValueGenerators.createGaussianSquareFilter);
                byte[,] processedImage = ProcessingFunctions.grayscaleDilateImage(workingImage, structElem);

                string imgPath = Path.Combine(basePath, "out", "task2", "images");
                string dataPath = Path.Combine(basePath, "out", "task2", "data");

                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                ImageData data = new ImageData(processedImage);
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_C" + (i + 1) + ".json"), jsonString);

                Image outputImage = HelperFunctions.convertToImage(processedImage);

                outputImage.Save(Path.Combine(imgPath, "C" + (i + 1) + ".bmp"), ImageFormat.Bmp);

            }
        }
    }
}