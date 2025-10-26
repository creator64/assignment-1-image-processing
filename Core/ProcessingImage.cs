using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using INFOIBV.Helper_Code;
using System.Numerics;
using System.Windows.Forms;
using INFOIBV.Core.TemplateMatching;

namespace INFOIBV.Core
{

    interface IImage
    {
        Bitmap getImage();
    }
    
    public class ProcessingImage : IImage
    {
        public readonly int width;
        public readonly int height;
        protected byte[,] inputImage;
        
        public ProcessingImage(byte[,] inputImage)
        {
            width = inputImage.GetLength(0);
            height = inputImage.GetLength(1);
            this.inputImage = inputImage;
        }

        public byte[,] toArray()
        {
            return inputImage;
        }
        
        /*
         * invertImage: invert a single channel (grayscale) image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        public ProcessingImage invertImage()
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[width, height];
            
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
                tempImage[x, y] = (byte)(255 - inputImage[x, y]);

            return new ProcessingImage(tempImage);
        }


        /*
         * adjustContrast: create an image with the full range of intensity values used
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        public ProcessingImage adjustContrast()
        {
            int alow = int.MaxValue, ahigh = int.MinValue;
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
            { 
                if (inputImage[x, y] > ahigh) ahigh = inputImage[x, y];
                if (inputImage[x, y] < alow) alow = inputImage[x, y]; 
            }

            // create temporary grayscale image
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    tempImage[x, y] = (byte)((inputImage[x, y] - alow) * 255 / (ahigh - alow));

            return new ProcessingImage(tempImage);
        }


        /*
         * createGaussianFilter: create a Gaussian filter of specific square size and with a specified sigma
         * input:   size                length and width of the Gaussian filter (only odd sizes)
         *          sigma               standard deviation of the Gaussian distribution
         * output:                      Gaussian filter
         */
        public static float[,] createGaussianFilter(byte size, float sigma)
        {
            if (size % 2 == 0) throw new Exception("size of gaussian filter cannot be even");
            int halfSize = size / 2;
            
            // create temporary grayscale image
            float[,] filter = new float[size, size];

            for (int x = -halfSize; x <= halfSize; x++) for (int y = -halfSize; y <= halfSize; y++)
                filter[x + halfSize, y + halfSize] = (float)Math.Exp(
                    (-Math.Pow(x, 2) - Math.Pow(y, 2)) / (2 * Math.Pow(sigma, 2))
                );

            float sum = 0;
            for (int x = 0; x < size; x++) for (int y = 0; y < size; y++)
                sum += filter[x, y];
            
            for (int x = 0; x < size; x++) for (int y = 0; y < size; y++)
                filter[x, y] /= sum;

            return filter;
        }


        /*
         * convolveImage: apply linear filtering of an input image
         * input:   inputImage          single-channel (byte) image
         *          filter              linear kernel
         * output:                      single-channel (byte) image
         */
        public ProcessingImage convolveImage(float[,] filter)
        {
            Padder padder = new CopyPerimeterPadder(filter);
            float[,] tempImage = HelperFunctions.applyUnevenFilter(inputImage, filter, padder);

            return new ProcessingImage(ConverterMethods.convertToBytes(tempImage));
        }


        /*
         * edgeMagnitude: calculate the image derivative of an input image and a provided edge kernel
         * input:   inputImage          single-channel (byte) image
         *          horizontalKernel    horizontal edge kernel
         *          verticalKernel      vertical edge kernel
         * output:                      single-channel (byte) image
         */
        public ProcessingImage edgeMagnitude(float[,] horizontalKernel, float[,] verticalKernel)
        {
            // create temporary grayscale image
            byte[,] resultImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            float[,] tempImage = new float[inputImage.GetLength(0), inputImage.GetLength(1)];

            #region test values
            //change these around later for the horizontalKernel and verticalKernel parameters
            
            #endregion

            Padder horizontalPadder = new CopyPerimeterPadder(horizontalKernel);
            Padder verticalPadder = new CopyPerimeterPadder(verticalKernel);

            float[,] Dx = HelperFunctions.applyUnevenFilter(inputImage, horizontalKernel, horizontalPadder);
            float[,] Dy = HelperFunctions.applyUnevenFilter(inputImage, verticalKernel, verticalPadder);

            float min = float.MaxValue;
            float max = float.MinValue;

            // calculate edge magnitude
            for (int i = 0; i < tempImage.GetLength(0); i++) for (int j = 0; j < tempImage.GetLength(1); j++)
            {
                float result = (float)Math.Sqrt(Math.Pow(Dx[i, j], 2) + Math.Pow(Dy[i, j], 2));

                if (result < min) min = result;
                if (result > max) max = result;

                tempImage[i, j] = result;
            }

            //normalise edge magnitudes to range 0 - 255
            // and copy to the resulting image
            float trueRange = max - min; //the entire range of values that's currently used

            for (int i = 0; i < tempImage.GetLength(0); i++) for (int j = 0; j < tempImage.GetLength(1); j++)
                resultImage[i, j] = (byte)(255.0f * ((tempImage[i, j] - min) / trueRange));

            return new ProcessingImage(resultImage);
        }
        

        /*
         * thresholdImage: threshold a grayscale image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image with on/off values
         */
        public ProcessingImage thresholdImage(byte threshold)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[width, height];
            
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
                tempImage[x, y] = (byte)(inputImage[x, y] > threshold ? 255 : 0);

            return new ProcessingImage(tempImage);
        }

        public ProcessingImage halfThresholdImage(byte threshold)
        {
            // create temporary grayscale image
            byte[,] tempImage = new byte[width, height];
            
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++)
                tempImage[x, y] = (byte)(inputImage[x, y] > threshold ? inputImage[x, y] : 0);

            return new ProcessingImage(tempImage);
        }

        // Binary morphology

        /*
         * binaryErodeImage: perform binary erosion on a binary image using a structuring element
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after erosion
         */
        public ProcessingImage binaryErodeImage(bool[,] structElem)
        {
            if (getImageData().amountDistinctValues > 2)
            {
                MessageBox.Show("You can only perform binary morphology over binary images. Threshold this image first");
                return new ProcessingImage(inputImage);
            }
            else
            {
                HashSet<Vector2> imageSet = BinaryMorphologyHelpers.pointSetFromImage(inputImage);
                HashSet<Vector2> structSet = BinaryMorphologyHelpers.pointSetFromFilter(structElem);

                HashSet<Vector2> resultSet = new HashSet<Vector2>();

                foreach (Vector2 pixel in imageSet)
                {
                    bool pixelMayRemain = true;
                    foreach (Vector2 element in structSet)
                    {
                        if (!imageSet.Contains(pixel + element))
                        {
                            pixelMayRemain = false;
                            break;
                        }

                    }

                    if (pixelMayRemain) resultSet.Add(pixel);
                }

                return new ProcessingImage(
                    BinaryMorphologyHelpers.pointSetToImage(
                        resultSet, new Vector2(inputImage.GetLength(0), inputImage.GetLength(1)))
                    );
            }
        }

        /*
         * binaryDilateImage: perform binary dilation on a binary image using a structuring element
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after dilation
         */
        public ProcessingImage binaryDilateImage(bool[,] structElem)
        {
            
            HashSet<Vector2> imageSet = BinaryMorphologyHelpers.pointSetFromImage(inputImage);
            HashSet<Vector2> structSet = BinaryMorphologyHelpers.pointSetFromFilter(structElem);

            HashSet<Vector2> resultSet = new HashSet<Vector2>();
            foreach(Vector2 pixel in imageSet)
                foreach(Vector2 element in structSet)
                    resultSet.Add(pixel + element);

            return new ProcessingImage(
                BinaryMorphologyHelpers.pointSetToImage(
                    resultSet, new Vector2(inputImage.GetLength(0), inputImage.GetLength(1))));
        }

        /*
         * binaryOpen
         * Image: perform binary opening on a binary image
         * input:   inputImage          single-channel (byte) binary image 
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after opening
         */
        public ProcessingImage binaryOpenImage(bool[,] structElem)
        {
            return binaryErodeImage(structElem).binaryDilateImage(structElem);
        }

        /*
         * binaryCloseImage: perform binary closing on a binary image
         * input:   inputImage          single-channel (byte) binary image
         *          structElem          binary structuring element (true = foreground)
         * output:                      single-channel (byte) binary image after closing
         */
        public ProcessingImage binaryCloseImage(bool[,] structElem)
        {
            return binaryDilateImage(structElem).binaryErodeImage(structElem);
        }

        // Grayscale morphology

        /*
         * grayscaleErodeImage: perform grayscale erosion on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element 
         * output:                      single-channel (byte) grayscale image after erosion
         */
        public ProcessingImage grayscaleErodeImage(int[,] structElem)
        {
            float[,] floatStructElem = HelperFunctions.floatifyIntArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Min<float>, (x, y) => x - y);

            byte[,] output = ConverterMethods.convertToBytes(floatyresult);

            return new ProcessingImage(output);
        }

        /*
         * grayscaleDilateImage: perform grayscale dilation on a grayscale image using a structuring element
         * input:   inputImage          single-channel (byte) grayscale image
         *          structElem          integer structuring element
         * output:                      single-channel (byte) grayscale image after dilation
         */
        public ProcessingImage grayscaleDilateImage(int[,] structElem)
        {
            float[,] floatStructElem = HelperFunctions.floatifyIntArray(structElem);
            Padder padder = new ConstantValuePadder(floatStructElem, 0);

            float[,] floatyresult = HelperFunctions.applyMorphologicalFilter(inputImage, floatStructElem, padder, Enumerable.Max<float>, (x, y) => x + y);

            byte[,] output = ConverterMethods.convertToBytes(floatyresult);

            return new ProcessingImage(output);
        }
        
        /*
         * histogramEqualization: create the histogram-equalized version of the image
         * input:   inputImage          single-channel (byte) image
         * output:                      single-channel (byte) image
         */
        public ProcessingImage histogramEqualization()
        {
            int[] cumulativeHistogram = HelperFunctions.calculateCumulativeHistogram(inputImage);
            int amountOfPixels = inputImage.GetLength(0) * inputImage.GetLength(1);
            int K = cumulativeHistogram.Length;
            
            int[] mappedPixels = new int[K];
            for (int i = 0; i < K; i++)
                mappedPixels[i] = (int)Math.Floor((K - 1d) * cumulativeHistogram[i] / amountOfPixels);

            byte[,] outputImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];
            for (int x = 0; x < inputImage.GetLength(0); x++) for (int y = 0; y < inputImage.GetLength(1); y++)
                outputImage[x, y] = (byte)mappedPixels[inputImage[x, y]];

            return new ProcessingImage(outputImage);
        }

        public ProcessingImage medianFilter(int filterSize)
        {
            float[,] filter = FilterGenerators.createSquareFilter(filterSize, (int _, int __, float[,] ___) => 0);
            Padder padder = new ConstantValuePadder(filter, 0);
            int paddingWidth = padder.paddingWidth, paddingHeight = padder.paddingHeight;

            Func<int, int, byte[,], float> f = (i, j, paddedImage) =>
            {
                int[] intensities = new int[filterSize * filterSize];
                int a = 0;
                for (int k = -paddingWidth; k <= paddingWidth; k++)
                for (int l = -paddingHeight; l <= paddingHeight; l++)
                    intensities[a++] = paddedImage[i + k, j + l];

                Array.Sort(intensities);

                return intensities[intensities.Length / 2];
            };

            return new ProcessingImage(
                    HelperFunctions.applyNonLinearFilter(inputImage, padder, f)
                );
        }

        public Dictionary<Point, float> findMatchesBinary(ProcessingImage templateImage, int threshold = 0, bool checkEdges = false, List<Point> pointsToCheck = null)
        {
            DistanceStyle ds = new ManhattanDistance();
            float[,] distances = new ChamferDistanceTransform(inputImage, 3).toDistances(ds);
            int K = templateImage.getImageData().amountForegroundPixels;
            Dictionary<Point, float> scores = new Dictionary<Point, float>();
            
            for (int r = 0; r < width; r++)
            for (int s = 0; s < height; s++)
            {
                if ((width - r < templateImage.width || height - s < templateImage.height) && !checkEdges) continue;
                if (pointsToCheck != null && !pointsToCheck.Contains(new Point(r, s))) continue;
                
                float score = 0;
                for (int k = 0; k < templateImage.width; k++)
                for (int l = 0; l < templateImage.height; l++)
                {
                    if (templateImage.inputImage[k, l] != 255) continue;
                    int x = r + k, y = s + l; if (outOfBounds(x, y)) continue;
                    score += distances[x, y];
                }

                score /= K;

                if (score > threshold) scores.Add(new Point(r, s), score);
            }
            
            return scores;
        }

        public Point findBestMatchBinary(ProcessingImage templateImage)
        {
            Dictionary<Point, float> scores = findMatchesBinary(templateImage, 0);
            return scores.Aggregate(
                (s1, s2) => s1.Value < s2.Value ? s1 : s2)
            .Key;
        }

        public RGBImage visualiseBestMatchBinary(ProcessingImage templateImage)
        {
            Point bestMatch = findBestMatchBinary(templateImage);
            Bitmap output = getImage();
            
            // outlined square
            for (int i = bestMatch.X; i < bestMatch.X + templateImage.width; i++)
                output.SetPixel(i, bestMatch.Y, Color.Red);
            for (int i = bestMatch.X; i < bestMatch.X + templateImage.width; i++)
                output.SetPixel(i, bestMatch.Y + templateImage.height, Color.Red);
            for (int j = bestMatch.Y; j < bestMatch.Y + templateImage.height; j++)
                output.SetPixel(bestMatch.X, j, Color.Red);
            for (int j = bestMatch.Y; j < bestMatch.Y + templateImage.height; j++)
                output.SetPixel(bestMatch.X + templateImage.width, j, Color.Red);

            return new RGBImage(output);
        }

        protected bool outOfBounds(int x, int y)
        {
            return x < 0 || x >= width || y < 0 || y >= height;
        }

        public RegionalProcessingImage toRegionalImage(ImageRegionFinder regionFinder)
        {
            return new RegionalProcessingImage(inputImage, regionFinder);
        }

        public HoughTransform toHoughTransform(int thetaDetail, int rDetail)
        {
            return new HoughTransform(inputImage, thetaDetail, rDetail);
        }

        public Bitmap getImage()
        {
            Bitmap OutputImage = new Bitmap(inputImage.GetLength(0), inputImage.GetLength(1)); // create new output image
            for (int x = 0; x < inputImage.GetLength(0); x++)             // loop over columns
            for (int y = 0; y < inputImage.GetLength(1); y++)         // loop over rows
            {
                Color newColor = Color.FromArgb(inputImage[x, y], inputImage[x, y], inputImage[x, y]);
                OutputImage.SetPixel(x, y, newColor);                  // set the pixel color at coordinate (x,y)
            }

            return OutputImage;
        }

        protected ImageData getImageData()
        {
            return new ImageData(inputImage);
        }
    }

    public class RGBImage : IImage
    {
        public Bitmap rgbImage { get; private set; }
        public RGBImage(Bitmap inputRGB)
        {
            this.rgbImage = inputRGB;
        }

        public Bitmap getImage()
        {
            return rgbImage;
        }
    }
}