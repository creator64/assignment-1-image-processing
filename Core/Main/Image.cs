using System;
using System.Collections.Generic;
using System.Drawing;

namespace INFOIBV.Core.Main
{
    public abstract class Image
    {
        public int width;
        public int height;
        public abstract Bitmap getImage();

        public RGBImage drawRectangles(List<Rectangle> rectangles, Color color = new Color())
        {
            if (color.IsEmpty) color = Color.Red;
            Bitmap output = new Bitmap(getImage());

            foreach (Rectangle rectangle in rectangles)
            {
                double endX = Math.Min(rectangle.X + rectangle.Width, width - 1);
                double endY = Math.Min(rectangle.Y + rectangle.Height, height - 1);

                for (int i = rectangle.X; i <= endX; i++)
                {
                    output.SetPixel(i, rectangle.Y, color);
                    output.SetPixel(i, (int)endY, color);
                }

                for (int j = rectangle.Y; j <= endY; j++)
                {
                    output.SetPixel(rectangle.X, j, color);
                    output.SetPixel((int)endX, j, color);
                }
            }

            return new RGBImage(output);
        }
    }
}