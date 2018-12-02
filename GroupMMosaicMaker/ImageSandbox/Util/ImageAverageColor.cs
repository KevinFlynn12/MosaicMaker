using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Util
{
    public static class ImageAverageColor
    {
        public static Color FindAverageColorForSelectedArea(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
            int XStoppingPoint)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;

            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);
                    totalRed += pixelColor.R;
                    totalBlue += pixelColor.B;
                    totalGreen += pixelColor.G;
                }
            }

            var averageRed = totalRed / pixelCount;
            var averageBlue = totalBlue / pixelCount;
            var averageGreen = totalGreen / pixelCount;

            var newColor = new Color();
            newColor.R = (byte)averageRed;
            newColor.B = (byte)averageBlue;
            newColor.G = (byte)averageGreen;
            return newColor;
        }

        public static Color FindAverageColorOfEntireImage(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;


            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageHeight; x++)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, y, x, imageWidth, imageHeight);
                    totalRed += pixelColor.R;
                    totalBlue += pixelColor.B;
                    totalGreen += pixelColor.G;
                }
            }

            var averageRed = totalRed / pixelCount;
            var averageBlue = totalBlue / pixelCount;
            var averageGreen = totalGreen / pixelCount;

            var newColor = new Color();
            newColor.R = (byte) averageRed;
            newColor.B = (byte) averageBlue;
            newColor.G = (byte) averageGreen;
            return newColor;

        }






    }
}
