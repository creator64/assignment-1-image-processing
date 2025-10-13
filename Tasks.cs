using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using INFOIBV.Core;
using INFOIBV.Helper_Code;
using System.Text.Json;
using System.Diagnostics;
using System.Security.Policy;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace INFOIBV
{
    public class Tasks
    {
        public void Task1(string path, decimal sigma)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = enviroment;

            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = ConverterMethods.convertBitmapToColor(InputImage);
            byte[,] workingImage = ConverterMethods.convertToGrayscale(Image);
            ProcessingImage processingImage = new ProcessingImage(workingImage).adjustContrast();

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
                ProcessingImage processedImage = Pipelines.GaussianFilterAndEdgeDetection(sigma, sizes[i], horizontalKernel,
                    verticalKernel, processingImage);
                string outputPath = Path.Combine(basePath, "out", "task1", "sigma=" + sigma);

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                Image outputImage = processedImage.convertToImage();
                outputImage.Save(Path.Combine(outputPath, "B" + (i + 1) + ".bmp"), ImageFormat.Bmp);
                outputImage.Save(Path.Combine(outputPath, "B" + (i + 1) + ".png"), ImageFormat.Png); //also create a png image for the report

            }
        }

        public void Task2(string path)
        {
            var enviroment = Environment.CurrentDirectory;
            string basePath = enviroment;
            
            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = ConverterMethods.convertBitmapToColor(InputImage);
            byte[,] workingImage = ConverterMethods.convertToGrayscale(Image);
            ProcessingImage processingImage = new ProcessingImage(workingImage).adjustContrast();

            int[] sizes = { 3, 7, 11, 15, 19 };

            for (int i = 0; i < sizes.Length; i++)
            {
                int[,] structElem = FilterGenerators.createSquareFilter<int>(sizes[i], FilterValueGenerators.createUniformSquareFilter);
                ProcessingImage processedImage = processingImage.grayscaleDilateImage(structElem);

                string imgPath = Path.Combine(basePath, "out", "task2", "images");
                string dataPath = Path.Combine(basePath, "out", "task2", "data");

                if (!Directory.Exists(imgPath))
                    Directory.CreateDirectory(imgPath);
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);

                TaskData<int> data = new TaskData<int>(structElem, processedImage.toArray());
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_C" + (i + 1) + ".json"), jsonString);

                Image outputImage = processedImage.convertToImage();

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
            Color[,] Image = ConverterMethods.convertBitmapToColor(InputImage);
            byte[,] workingImage = ConverterMethods.convertToGrayscale(Image);
            ProcessingImage imageF = new ProcessingImage(workingImage).thresholdImage(127);

            string imgPath = Path.Combine(basePath, "out", "task3", "images");
            string dataPath = Path.Combine(basePath, "out", "task3", "data");

            if (!Directory.Exists(imgPath))
                Directory.CreateDirectory(imgPath);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            Image img_imageF = imageF.convertToImage();
            img_imageF.Save(Path.Combine(imgPath, "Image_F.bmp"), ImageFormat.Bmp);

            int[] sizes = { 3, 13, 23, 33 };

            for (int i = 0; i < sizes.Length; i++)
            {
                bool[,] structElem = FilterGenerators.createSquareFilter<bool>(sizes[i], FilterValueGenerators.createUniformBinaryStructElem);
                ProcessingImage processedImage = imageF.binaryCloseImage(structElem);

                TaskData<bool> data = new TaskData<bool>(structElem, processedImage.toArray());
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(Path.Combine(dataPath, "image_data_G" + (i + 1) + ".json"), jsonString);

                Image outputImage = processedImage.convertToImage();

                outputImage.Save(Path.Combine(imgPath, "G" + (i + 1) + ".bmp"), ImageFormat.Bmp);
                outputImage.Save(Path.Combine(imgPath, "G" + (i + 1) + ".png"), ImageFormat.Png); //also create a png image for the report

            }
        }

        public void BinaryPipeline(string path)
        {
            var enviroment = System.Environment.CurrentDirectory;
            string basePath = enviroment;
            //basePath = "C:\\Users\\Dangual\\Documents\\UU\\Beeldverwerking\\assignment-1-image-processing";

            Debug.WriteLine(Path.Combine(basePath, path));
            Bitmap InputImage = new Bitmap(Path.Combine(basePath, path));
            Color[,] Image = ConverterMethods.convertBitmapToColor(InputImage);
            byte[,] workingImage = ConverterMethods.convertToGrayscale(Image);
            ProcessingImage processingImage = new ProcessingImage(workingImage);

            List<BinaryPipelineConfig> configs = new List<BinaryPipelineConfig>{
                new BinaryPipelineConfig("t_mag_too_low", 2, 2, 90, 50, 10, 7, 75),             // compensate lower t_mag with bigger t_peak
                new BinaryPipelineConfig("t_peak_compensation", 2, 2, 160, 50, 10, 7, 75),      // compensate lower t_mag with bigger t_peak
                new BinaryPipelineConfig("t_peak_too_low", 2, 2, 40, 50, 5, 2, 70),             // example of too low a t_peak
                new BinaryPipelineConfig("detail_loss", 2, 2, 150, 50, 25, 32, 90),             // too big of a t_peak for too high a t_mag, many details lost.
                new BinaryPipelineConfig("minSegLength_too_high", 2, 2, 120, 50, 30, 22, 70),   // example of too big a minimal segment length
                new BinaryPipelineConfig("perfection", 2, 2, 80, 50, 20, 7, 90)                 // perfect
            };

            string imgPath = Path.Combine(basePath, "out", "task4", "images");
            string dataPath = Path.Combine(basePath, "out", "task4", "data");

            if (!Directory.Exists(imgPath))
                Directory.CreateDirectory(imgPath);
            if (!Directory.Exists(dataPath))
                Directory.CreateDirectory(dataPath);

            foreach (BinaryPipelineConfig config in configs)
            {
                ProcessingImage processedImage = Pipelines.BinaryPipeline(processingImage,
                                                                          processingImage.width * config.thetaDetailFactor,
                                                                          processingImage.height * config.rDetailFactor,
                                                                          config.minIntensity,
                                                                          config.minSegLength,
                                                                          config.maxGap,
                                                                          config.t_mag,
                                                                          config.t_peak,
                                                                          config.regionFinder);
                string jsonString = JsonSerializer.Serialize(config);
                File.WriteAllText(Path.Combine(dataPath, "config_" + config.name + ".json"), jsonString);

                Image output = processedImage.convertToImage();

                output.Save(Path.Combine(imgPath, config.name + ".png"), ImageFormat.Png);
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

    public class BinaryPipelineConfig
    {
        public string name { get; private set; }
        public int thetaDetailFactor { get; private set; }
        public int rDetailFactor { get; private set; }
        public byte t_mag { get; private set; }
        public byte t_peak { get; private set; }
        public byte minIntensity { get; private set; }
        public ushort minSegLength { get; private set; }
        public ushort maxGap { get; private set; }

        public ImageRegionFinder regionFinder { get; private set; }

        public BinaryPipelineConfig(string name, int thetaDetailFactor, int rDetailFactor, byte t_peak, byte minIntensity, ushort minSegLength, ushort maxGap, byte t_mag, ImageRegionFinder regionFinder = null)
        {
            this.name = name;
            this.thetaDetailFactor = thetaDetailFactor;
            this.rDetailFactor = rDetailFactor;
            this.t_mag = t_mag;
            this.t_peak = t_peak;
            this.minIntensity = minIntensity;
            this.minSegLength = minSegLength;
            this.maxGap = maxGap;
            this.regionFinder = regionFinder;

            if (regionFinder != null) this.regionFinder = regionFinder;
            else this.regionFinder = new FloodFill();
        }
    }
    public class GrayscalePipelineConfig
    {
        public string name { get; private set; }
        public int thetaDetailFactor { get; private set; }
        public int rDetailFactor { get; private set; }
        public byte t_peak { get; private set; }
        public byte minIntensity { get; private set; }
        public ushort minSegLength { get; private set; }
        public ushort maxGap { get; private set; }
        public ImageRegionFinder regionFinder { get; private set; }

        public GrayscalePipelineConfig(string name, byte thetaDetailFactor, byte rDetailFactor, byte t_peak, byte minIntensity, ushort minSegLength, ushort maxGap, ImageRegionFinder regionFinder = null)
        {
            this.name = name;
            this.thetaDetailFactor = thetaDetailFactor;
            this.rDetailFactor = rDetailFactor;
            this.t_peak = t_peak;
            this.minIntensity = minIntensity;
            this.minSegLength = minSegLength;
            this.maxGap = maxGap;

            if (regionFinder != null) this.regionFinder = regionFinder;
            else this.regionFinder = new FloodFill();
        }
    }
}