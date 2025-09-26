using System.Drawing;
using INFOIBV.Helper_Code;

namespace INFOIBV
{
    public class Tasks
    {
        public void Task1(string path, int sigma)
        {
            Bitmap InputImage = new Bitmap(path);
            Color[,] Image = HelperFunctions.convertBitmapToColor(InputImage);
            byte[,] workingImage = HelperFunctions.convertToGrayscale(Image);
            
            int[] sizes = new[] { 3, 7, 11, 15, 19 };
        }
    }
}