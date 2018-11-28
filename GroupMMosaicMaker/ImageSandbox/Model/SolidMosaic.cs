using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Model
{
    class SolidMosaic
    {
        public bool HasGrid { get; set; }
        public byte[] SourcePixels { get; set; }

        public SolidMosaic(bool hasGrid, byte[] sourcePixels)
        {
            this.HasGrid = hasGrid;
            this.SourcePixels = sourcePixels;
        }

        public SolidMosaic()
        {

        }

        public void CreateSolidMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize, bool isGrid)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, YStoppingPoint, x, XStoppingPoint, isGrid);

                    x += blockSize;
                }
                y += blockSize;
            }
        }

        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint, int XStoppingPoint, bool isGrid)
        {
            var averageColor =
                AnalyzeImage.FindAverageColor(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    YStoppingPoint, startingXPoint, XStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    var pixelColor = AnalyzeImage.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);

                    if (isGrid)
                    {
                        if (currentYPoint == startingYPoint || YStoppingPoint == currentYPoint
                                                || currentXPoint == startingXPoint || XStoppingPoint == currentXPoint)
                        {
                            pixelColor = Colors.White;

                        }
                        else
                        {
                            pixelColor.R = averageColor.R;
                            pixelColor.B = averageColor.B;
                            pixelColor.G = averageColor.G;
                        }
                    }
                    else
                    {
                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;
                    }


                    this.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight);
                }
            }
        }
        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }
        private int UpdateStoppingPoint(uint maxValue, int coordinate, int blockSize)
        {
            var CoordinateStoppingPoint = coordinate + blockSize;
            if (CoordinateStoppingPoint > maxValue)
            {
                CoordinateStoppingPoint = (int)maxValue;
            }

            return CoordinateStoppingPoint;
        }
    }
}
