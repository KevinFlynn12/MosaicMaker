using Windows.UI;

namespace ImageSandbox.Util
{
    public static class ImageAverageColor
    {
        #region Methods

        /// <summary>
        ///     Finds the average color for selected area.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="startingYPoint">The starting y point.</param>
        /// <param name="yStoppingPoint">The y stopping point.</param>
        /// <param name="startingXPoint">The starting x point.</param>
        /// <param name="xStoppingPoint">The x stopping point.</param>
        /// <returns> The average color for that area </returns>
        public static Color FindAverageColorForSelectedArea(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            int startingYPoint, int yStoppingPoint, int startingXPoint,
            int xStoppingPoint)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;

            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixel(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);
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

        /// <summary>
        ///     Finds the average black and white color for selected area.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="startingYPoint">The starting y point.</param>
        /// <param name="yStoppingPoint">The y stopping point.</param>
        /// <param name="startingXPoint">The starting x point.</param>
        /// <param name="xStoppingPoint">The x stopping point.</param>
        /// <returns>Black or White depending on average color value</returns>
        public static Color FindAverageBlackAndWhiteColorForSelectedArea(byte[] sourcePixels, uint imageWidth,
            uint imageHeight,
            int startingYPoint, int yStoppingPoint, int startingXPoint,
            int xStoppingPoint)
        {
            var totalBlack = 0.0;
            var totalWhite = 0.0;

            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixel(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);
                    var averageBlack = pixelColor.R + pixelColor.B + pixelColor.G / 3;
                    if (averageBlack >= 127.5)
                    {
                        totalWhite++;
                    }
                    else
                    {
                        totalBlack++;
                    }
                }
            }

            var newColor = new Color();
            if (totalBlack > totalWhite)
            {
                newColor.R = 0;
                newColor.B = 0;
                newColor.G = 0;
            }
            else
            {
                newColor.R = 255;
                newColor.B = 255;
                newColor.G = 255;
            }

            return newColor;
        }

        /// <summary>
        ///     Finds the average color of entire image.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <returns>The average color for the entire image</returns>
        public static Color FindAverageColorOfEntireImage(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;

            for (var y = 0; y < imageHeight; y++)
            {
                for (var x = 0; x < imageHeight; x++)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixel(sourcePixels, y, x, imageWidth, imageHeight);
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

        #endregion
    }
}