using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using INFOIBV;
using INFOIBV.Helper_Code;
using static System.Net.Mime.MediaTypeNames;
namespace INFOIBV.Core
{
    public static class Pipelines
    {
        public static ProcessingImage GaussianFilterAndEdgeDetection(
                decimal sigma, decimal gaussianMatrixSize, 
                float[,] horizontalKernel, float[,] verticalKernel,
                ProcessingImage processingImage
            )
        {
            float[,] gaussianFilter = ProcessingImage.createGaussianFilter((byte)gaussianMatrixSize, (float)sigma);
            return processingImage
                .convolveImage(gaussianFilter)
                .edgeMagnitude(horizontalKernel, verticalKernel)
                .thresholdImage(30);
        }

        public static ProcessingImage Task4(ProcessingImage processingImage, byte t_mag, byte t_peak)
        {
            float[,] gaussianFilter = ProcessingImage.createGaussianFilter(5, 2.25f);
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
            processingImage.convolveImage(gaussianFilter).edgeMagnitude(horizontalKernel, verticalKernel).thresholdImage(t_mag);

            return null;
            //processingImage.houghTransform();
        }
    }
}