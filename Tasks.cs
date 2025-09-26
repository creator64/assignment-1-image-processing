using System.Drawing;
using INFOIBV.Helper_Code;

namespace INFOIBV
{
    public class Tasks
    {
        public void Task1(string path, int sigma)
        {
            Bitmap InputImage = new Bitmap(path);
            int[] sizes = new[] { 3, 7, 11, 15, 19 };
            
            Color[,] Image = new Color[InputImage.Size.Width, InputImage.Size.Height]; // create array to speed-up operations (Bitmap functions are very slow)

            // copy input Bitmap to array            
            for (int x = 0; x < InputImage.Size.Width; x++)                 // loop over columns
            for (int y = 0; y < InputImage.Size.Height; y++)            // loop over rows
                Image[x, y] = InputImage.GetPixel(x, y);                // set pixel color in array at (x,y)

            // execute image processing steps
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);
        }
    }
}