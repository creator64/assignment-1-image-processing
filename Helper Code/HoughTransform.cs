using INFOIBV.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace INFOIBV.Helper_Code
{
    public class HoughTransformer
    {
        int thetaDetail;
        int rDetail;
        byte[,] image;
        int minIntensity;

        int imageWidth { get { return image.GetLength(0); } }
        int imageHeight { get { return image.GetLength(1); } }
        
        float Rmax { get { return 0.5f * (float)Math.Sqrt((imageWidth * imageWidth) + (imageHeight * imageHeight)); } }

        float deltaTheta { get { return (float)(Math.PI) / thetaDetail; } }

        float deltaR { get { return (float)(2 * Rmax) / rDetail; } }

        (int X, int Y) centerPoint { get { return (imageWidth / 2, imageHeight / 2); } }

        
        public HoughTransformer(byte[,] image, int thetaDetail, int rDetail, byte minIntensity = 1)
        {
            this.image = image;
            this.thetaDetail = thetaDetail;
            this.rDetail = rDetail;
            this.minIntensity = minIntensity;
        }

        public float getRMax(int imageWidth, int imageHeight)
        {
            return 0.5f * (float)Math.Sqrt((imageWidth * imageWidth) + (imageHeight * imageHeight));
        }

        public Func<float, float> generateWaveFunction(int x, int y)
        {
            return (theta) => (float)(x * Math.Cos(theta) + y * Math.Sin(theta));
        }

        public ProcessingImage houghTransform()
        {
            float[,] accumulatorArray = new float[thetaDetail + 1, rDetail + 1];

            for (int i = 0; i < imageWidth; i++)
            {
                for (int j = 0; j < imageHeight; j++)
                {
                    if (image[i, j] < minIntensity) continue;   //check if the pixel in the image is considered "on"

                    //for this exercise the center point should be in the center of the image.
                    int x = i - centerPoint.X;
                    int y = i - centerPoint.Y;

                    Func<float, float> hnf = generateWaveFunction(x, y);

                    for (int indexTheta = 0; indexTheta < thetaDetail; indexTheta++)
                    {
                        float theta = indexTheta * deltaTheta;
                        float r = hnf(theta);
                        int indexR = (rDetail / 2) + (int)Math.Floor(r / deltaR);

                        if (indexR >= 0 && indexR < accumulatorArray.GetLength(1) && accumulatorArray[indexTheta, indexR] < float.MaxValue)
                            accumulatorArray[indexTheta, indexR] += 1;
                        else
                        {
                            //Debug.WriteLine($"rmax: {Rmax}");
                            //Debug.WriteLine($"theta index: {indexTheta}");
                            //Debug.WriteLine($"r index: {indexR}");
                            //Debug.WriteLine($"accumulator array dimensions: ({accumulatorArray.GetLength(0)}, {accumulatorArray.GetLength(1)})");
                        }
                    }
                }
            }

            byte[,] bytifiedAccumulator = ConverterMethods.convertToBytes(accumulatorArray);

            return new ProcessingImage(bytifiedAccumulator);
        }
    }
}
