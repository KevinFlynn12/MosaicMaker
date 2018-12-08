using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    internal class MosaicImage
    {
        #region Properties
        

        public StorageFile ImageFile { get; set; }

        public int BlockSize { get; set; }

        #endregion

        #region Constructors

        public MosaicImage(StorageFile imageFile, int blockSize)
        {
            this.ImageFile = imageFile;
            this.BlockSize = blockSize;
        }

        #endregion

        #region Methods

        private async Task<BitmapImage> CreateAImageFromFile(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        public void CreateSolidMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight - 1, y);

                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth - 1, x);
                
                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint, false);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }
        public void CreateTriangleMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight - 1, y);

                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth - 1, x);

                    this.TriangleMosaic(sourcePixels, imageWidth, imageHeight, x, y, xStoppingPoint, yStoppingPoint);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }


        public void CreatePictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            ImagePalette loadedImages)
        {
            for (var y = 0; y < imageHeight; y += this.BlockSize)
            {
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                for (var x = 0; x < imageWidth; x += this.BlockSize)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    this.setPictureMosaic(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x,
                        xStoppingPoint, loadedImages);
                }
            }
        }

        public List<Tuple<int, int>> FindTrianglePoints(uint imageWidth, uint imageHeight)
        {

            var triangleCoordinates = new List<Tuple<int, int>>();

            for (var x = 0; x <= imageHeight; x += this.BlockSize)
            {
                for (var y = 0; y <= imageHeight; y += this.BlockSize)
                {
                    var currentX = 0;
                    for (var currentXPoint = x;
                        currentXPoint < this.UpdateStoppingPoint(imageWidth, x);
                        currentXPoint++)
                    {
                        var currentY = 0;
                        for (var currentYPoint = y;
                            currentYPoint < this.UpdateStoppingPoint(imageHeight, y);
                            currentYPoint++)
                        {
                            if (currentYPoint == y || this.UpdateStoppingPoint(imageHeight, y) == currentYPoint
                                                   || currentXPoint == x ||
                                                   this.UpdateStoppingPoint(imageWidth, x) == currentXPoint)
                            {
                                var coordinate = new Tuple<int, int>(currentXPoint, currentYPoint);
                                triangleCoordinates.Add(coordinate);
                            }
                            else if (currentX == currentY)
                            {
                                var coordinate = new Tuple<int, int>(currentXPoint, currentYPoint);
                                triangleCoordinates.Add(coordinate);
                            }

                            currentY++;
                        }

                        currentX++;
                    }
                }
            }

            return triangleCoordinates;
        }

        private void setPictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            int startingYPoint, int yStoppingPoint,
            int startingXPoint, int xStoppingPoint, ImagePalette loadedImages)
        {
            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);

            var matchingImage = loadedImages.FindClosestMatchingImage(averageColor);

            var matchingImageY = 0;

            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                var matchingImageX = 0;
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    Color pixelColor;

                    pixelColor = matchingImage.ImageBitmap.GetPixel(matchingImageX, matchingImageY);

                    ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                        imageHeight);

                    matchingImageX++;
                }

                matchingImageY++;
            }
        }

        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint,
            int yStoppingPoint, int startingXPoint, int xStoppingPoint, bool isBlackAndWhite)
        {

            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);

                    if (isBlackAndWhite)
                    {
                        var averageBlack = averageColor.R + averageColor.B + averageColor.G / 3;
                        if (averageBlack >= 127.5)
                        {
                            pixelColor.R = 255;
                            pixelColor.B = 255;
                            pixelColor.G = 255;
                        }
                        else
                        {
                            pixelColor.R = 0;
                            pixelColor.B = 0;
                            pixelColor.G = 0;
                        }
                    }
                    else
                    {
                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;
                    }

                    ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                        imageHeight);
                }
            }
        }

        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var coordinateStoppingPoint = coordinate + this.BlockSize;
            if (coordinateStoppingPoint > maxValue)
            {
                coordinateStoppingPoint = (int) maxValue;
            }

            return coordinateStoppingPoint;
        }

        /// <summary>
        ///     Creates the black and white mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="isGrid">if set to <c>true</c> [is grid].</param>
        public void CreateBlackAndWhiteMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                        true);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }

        private void TriangleMosaic(byte[] sourcePixels, uint imageHeight, uint imageWidth, int xStart, int yStart,
            int xStopping, int yStoppingPoint)
        {
            var topTriangleColors = new List<Color>();
            var topTriangleCoordinates = new List<Tuple<int, int>>();
            var bottomTriangleColors = new List<Color>();
            var bottomTriangleCoordinates = new List<Tuple<int, int>>();
            var currentY = 0;
            for (var y = yStart; y < yStoppingPoint; y++)
            {
                var currentX = 0;
                for (var x = xStart; x < xStopping; x++)
                {
                    if (currentY < currentX)
                    {
                        topTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, x, y, imageWidth, imageHeight);
                        topTriangleColors.Add(color);
                    }
                    else if (currentY >= currentX)
                    {
                        bottomTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, x, y, imageWidth, imageHeight);
                        bottomTriangleColors.Add(color);
                    }

                    currentX++;
                }

                currentY++;
            }

            var topTotalRed = 0;
            var topTotalBlue = 0;
            var topTotalGreen = 0;
            foreach (var currentColor in topTriangleColors)
            {
                topTotalRed += currentColor.R;
                topTotalBlue += currentColor.B;
                topTotalGreen += currentColor.G;
            }

            var averageTopRed = (byte) (topTotalRed / topTriangleColors.Count);
            var averageTopBlue = (byte) (topTotalBlue / topTriangleColors.Count);
            var averageTopGreen = (byte) (topTotalGreen / topTriangleColors.Count);
            var newColor = new Color();
            newColor.R = averageTopRed;
            newColor.G = averageTopGreen;
            newColor.B = averageTopBlue;
            foreach (var coordinate in topTriangleCoordinates)
            {
                ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item1, coordinate.Item2, newColor, imageWidth,
                    imageHeight);
            }

            var bottomTotalRed = 0;
            var bottomTotalBlue = 0;
            var bottomTotalGreen = 0;
            foreach (var currentColor in bottomTriangleColors)
            {
                bottomTotalRed += currentColor.R;
                bottomTotalBlue += currentColor.B;
                bottomTotalGreen += currentColor.G;
            }

            var bottomAverageTopRed = (byte) (bottomTotalRed / bottomTriangleColors.Count);
            var bottomAverageTopBlue = (byte) (bottomTotalBlue / bottomTriangleColors.Count);
            var bottomAverageTopGreen = (byte) (bottomTotalGreen / bottomTriangleColors.Count);
            var newBottomColor = new Color();
            newBottomColor.R = bottomAverageTopRed;
            newBottomColor.G = bottomAverageTopGreen;
            newBottomColor.B = bottomAverageTopBlue;
            foreach (var coordinate in bottomTriangleCoordinates)
            {
                ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item1, coordinate.Item2, newBottomColor, imageWidth,
                    imageHeight);
            }
        }

        #endregion
        
    }
}