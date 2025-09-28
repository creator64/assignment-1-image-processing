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
            workingImage = ProcessingFunctions.adjustContrast(workingImage); //Image A

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
                outputImage.Save(Path.Combine(outputPath, "B" + (i + 1) + ".png"), ImageFormat.Png); //also create a png image for the report

            }
        }

        public void Task2(string path)
        {
            var enviroment = Environment.CurrentDirectory;
            string basePath = enviroment;
            
            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = HelperFunctions.convertBitmapToColor(InputImage);
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);
            workingImage = ProcessingFunctions.adjustContrast(workingImage);

            int[] sizes = { 3, 7, 11, 15, 19 };

            for (int i = 0; i < sizes.Length; i++)
            {
                int[,] structElem = FilterGenerators.createSquareFilter<int>(sizes[i], FilterValueGenerators.createUniformSquareFilter);
                byte[,] processedImage = ProcessingFunctions.grayscaleDilateImage(workingImage, structElem);

                string imgPath = Path.Combine(basePath, "out", "task2", "images");
                string dataPath = Path.Combine(basePath, "out", "task2", "data");

                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                TaskData<int> data = new TaskData<int>(structElem, processedImage);
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_C" + (i + 1) + ".json"), jsonString);

                Image outputImage = HelperFunctions.convertToImage(processedImage);

                outputImage.Save(Path.Combine(imgPath, "C" + (i + 1) + ".bmp"), ImageFormat.Bmp);
                outputImage.Save(Path.Combine(imgPath, "C" + (i + 1) + ".png"), ImageFormat.Png); //also create a png image for the report

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

            byte[,] imageF = ProcessingFunctions.thresholdImage(workingImage, 127);

            string imgPath = Path.Combine(basePath, "out", "task3", "images");
            string dataPath = Path.Combine(basePath, "out", "task3", "data");

            if (!Directory.Exists(imgPath))
                Directory.CreateDirectory(imgPath);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            Image img_imageF = HelperFunctions.convertToImage(imageF);
            img_imageF.Save(Path.Combine(imgPath, "Image_F.bmp"), ImageFormat.Bmp);

            int[] sizes = { 3, 13, 23, 33 };

            for (int i = 0; i < sizes.Length; i++)
            {
                bool[,] structElem = FilterGenerators.createSquareFilter<bool>(sizes[i], FilterValueGenerators.createUniformBinaryStructElem);
                byte[,] processedImage = ProcessingFunctions.binaryCloseImage(imageF, structElem);

                TaskData<bool> data = new TaskData<bool>(structElem, processedImage);
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_G" + (i + 1) + ".json"), jsonString);

                Image outputImage = HelperFunctions.convertToImage(processedImage);

                outputImage.Save(Path.Combine(imgPath, "G" + (i + 1) + ".bmp"), ImageFormat.Bmp);
                outputImage.Save(Path.Combine(imgPath, "G" + (i + 1) + ".png"), ImageFormat.Png); //also create a png image for the report

            }
        }
    }
    public class TaskData<T>
    {
        public T[] flattenedFilter { get; private set; }

        public int filterWidth { get; private set; }

        public int filterHeight { get; private set; }

        public ImageData imgData { get; private set; } 

        public TaskData(T[,] filter, byte[,] image)
        {
            flattenedFilter = new T[filter.LongLength];

            for(int i = 0; i < filter.GetLength(0); i++)
            {
                for(int j = 0; j < filter.GetLength(1); j++)
                {
                    flattenedFilter[i + (j * filter.GetLength(1))] = filter[i, j];
                }
            }

            filterWidth = filter.GetLength(0);
            filterHeight = filter.GetLength(1);


            imgData = new ImageData(image);
        }
    }
}