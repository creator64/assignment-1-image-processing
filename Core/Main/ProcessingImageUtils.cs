using System;
using System.Collections.Generic;
using System.Drawing;
using INFOIBV.Core.TemplateMatching;
using INFOIBV.Helper_Code;

namespace INFOIBV.Core.Main
{
    public partial class ProcessingImage
    {
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

        public TemplateMatchingImage toTemplateMatchingImage()
        {
            return new TemplateMatchingImage(inputImage);
        }

        public SubImage createSubImage((int X, int Y) startPos, (int X, int Y) endPos)
        {
            return SubImage.create(this, startPos, endPos);
        }

        public ProcessingImage resize(int newWidth, int newHeight)
        {
            return fromBitmap(
                new Bitmap(getImage(), new Size(newWidth, newHeight))
            );
        }

        public override Bitmap getImage()
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

        public ImageData getImageData()
        {
            return new ImageData(inputImage);
        }
        
        public byte[,] toArray()
        {
            return inputImage;
        }

        public static ProcessingImage fromBitmap(Bitmap image)
        {
            return new ProcessingImage(
                ConverterMethods.convertToGrayscale(
                    ConverterMethods.convertBitmapToColor(image)
                    )
                );
        }
    }
}