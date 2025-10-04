using System.Drawing;
using System.Windows.Forms;

namespace INFOIBV.Helper_Code
{
    public static class ConverterMethods
    {
        /// <summary>
        /// convert a 2D array of floats to a 2D array of bytes
        /// </summary>
        /// <param name="I">the 2D float array to be converted</param>
        public static byte[,] convertToBytes(float[,] I)
        {
            int width = I.GetLength(0), height = I.GetLength(1);
            byte[,] tempImage = new byte[width, height];
            for (int i = 0; i < width; i++) for (int j = 0; j < height; j++) 
                tempImage[i, j] = (byte)I[i, j];

            return tempImage;
        }
        
        public static byte[,] convertToGrayscale(Color[,] inputImage, ProgressBar progressBar = null)
        {
            // create temporary grayscale image of the same size as input, with a single channel
            byte[,] tempImage = new byte[inputImage.GetLength(0), inputImage.GetLength(1)];

            // process all pixels in the image
            for (int x = 0; x < inputImage.GetLength(0); x++)                 // loop over columns
            for (int y = 0; y < inputImage.GetLength(1); y++)            // loop over rows
            {
                Color pixelColor = inputImage[x, y];                    // get pixel color
                byte average = (byte)((pixelColor.R + pixelColor.B + pixelColor.G) / 3); // calculate average over the three channels
                tempImage[x, y] = average;                              // set the new pixel color at coordinate (x,y)
                if (progressBar != null) progressBar.PerformStep();                              // increment progress bar
            }

            return tempImage;
        }

        public static Color[,] convertBitmapToColor(Bitmap InputImage)
        {
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
            for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            return Image;
        }
    }
}