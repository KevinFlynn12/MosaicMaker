using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Model
{
    class AnalyzeImage
    {
        public static Color FindAverageColor(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
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
                    var pixelColor = getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);
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
        public static Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }


    }
}
