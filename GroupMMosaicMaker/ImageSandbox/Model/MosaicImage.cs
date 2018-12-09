using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    internal class MosaicImage
    {
        #region Data members

        private readonly uint imageHeight;
        private readonly uint imageWidth;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the image file.
        /// </summary>
        /// <value>
        /// The image file.
        /// </value>
        public StorageFile ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public int BlockSize { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MosaicImage" /> class.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="imageWidth">Width of the image.</param>
        public MosaicImage(StorageFile imageFile, int blockSize, uint imageHeight, uint imageWidth)
        {
            this.ImageFile = imageFile;
            this.BlockSize = blockSize;

            this.imageHeight = imageHeight;
            this.imageWidth = imageWidth;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates the solid mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="isBlackAndWhite">if set to <c>true</c> [is black and white].</param>
        public void CreateSolidMosaic(byte[] sourcePixels, bool isBlackAndWhite)
        {
            for (var y = 0; y < this.imageHeight; y += this.BlockSize)
            {
                for (var x = 0; x < this.imageWidth; x += this.BlockSize)
                {
                    var yStoppingPoint = this.updateStoppingPoint(this.imageHeight, y);
                    var xStoppingPoint = this.updateStoppingPoint(this.imageWidth, x);

                    this.setNewColorValue(sourcePixels, y, yStoppingPoint, x, xStoppingPoint, isBlackAndWhite);
                }
            }
        }

        /// <summary>
        ///     Creates the triangle mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="isBlackAndWhite">if set to <c>true</c> [is black and white].</param>
        public void CreateTriangleMosaic(byte[] sourcePixels, bool isBlackAndWhite)
        {
            for (var y = 0; y < this.imageHeight; y += this.BlockSize)
            {
                for (var x = 0; x < this.imageWidth; x += this.BlockSize)
                {
                    var yStoppingPoint = this.updateStoppingPoint(this.imageHeight, y);
                    var xStoppingPoint = this.updateStoppingPoint(this.imageWidth, x);

                    this.triangleMosaic(sourcePixels, x, y, xStoppingPoint, yStoppingPoint, isBlackAndWhite);
                }
            }
        }

        /// <summary>
        ///     Creates the picture mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="loadedImages">The loaded images.</param>
        /// <param name="useAllImages">if set to <c>true</c> [use all images].</param>
        public async Task CreatePictureMosaic(byte[] sourcePixels, ImagePalette loadedImages, bool useAllImages)
        {
            for (var y = 0; y < this.imageHeight; y += this.BlockSize)
            {
                var yStoppingPoint = this.updateStoppingPoint(this.imageHeight, y);

                for (var x = 0; x < this.imageWidth; x += this.BlockSize)
                {
                    var xStoppingPoint = this.updateStoppingPoint(this.imageWidth, x);

                    await this.setPictureMosaic(sourcePixels, y, yStoppingPoint, x,
                        xStoppingPoint, loadedImages, useAllImages);
                }
            }
        }

        private async Task setPictureMosaic(byte[] sourcePixels, int startingYPoint, int yStoppingPoint,
            int startingXPoint, int xStoppingPoint, ImagePalette loadedImages, bool useAllImages)
        {
            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, this.imageWidth, this.imageHeight,
                    startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);

            var matchingImage = loadedImages.FindClosestMatchingImage(averageColor);

            var matchingImageY = 0;

            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                var matchingImageX = 0;
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = matchingImage.ImageBitmap.GetPixel(matchingImageX, matchingImageY);

                    ImagePixel.SetPixel(sourcePixels, currentYPoint, currentXPoint, pixelColor, this.imageWidth,
                        this.imageHeight);

                    matchingImageX++;
                }

                matchingImageY++;
            }

            if (useAllImages)
            {
                loadedImages.Remove(matchingImage);

                if (!loadedImages.Any())
                {
                    loadedImages.RepopulateImagePalettte();
                    await loadedImages.ResizeAllImages(this.BlockSize);
                }
            }
        }

        private void setNewColorValue(byte[] sourcePixels, int startingYPoint,
            int yStoppingPoint, int startingXPoint, int xStoppingPoint, bool isBlackAndWhite)
        {
            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, this.imageWidth, this.imageHeight,
                    startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);
            var averageBlack = ImageAverageColor.FindAverageBlackAndWhiteColorForSelectedArea(sourcePixels,
                this.imageWidth, this.imageHeight, startingYPoint,
                yStoppingPoint, startingXPoint, xStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixel(sourcePixels, currentYPoint, currentXPoint,
                        this.imageWidth, this.imageHeight);

                    if (isBlackAndWhite)
                    {
                        pixelColor.R = averageBlack.R;
                        pixelColor.B = averageBlack.B;
                        pixelColor.G = averageBlack.G;
                    }
                    else
                    {
                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;
                    }

                    ImagePixel.SetPixel(sourcePixels, currentYPoint, currentXPoint, pixelColor, this.imageWidth,
                        this.imageHeight);
                }
            }
        }

        private int updateStoppingPoint(uint maxValue, int coordinate)
        {
            var coordinateStoppingPoint = coordinate + this.BlockSize;
            if (coordinateStoppingPoint > maxValue)
            {
                coordinateStoppingPoint = (int) maxValue;
            }

            return coordinateStoppingPoint;
        }

        private void triangleMosaic(byte[] sourcePixels, int xStart, int yStart,
            int xStopping, int yStoppingPoint, bool isBlackAndWhite)
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
                    var color = new Color();
                    if (currentY < currentX)
                    {
                        topTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        color = ImagePixel.GetPixel(sourcePixels, y, x, this.imageWidth, this.imageHeight);
                        topTriangleColors.Add(color);
                    }
                    else
                    {
                        bottomTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        color = ImagePixel.GetPixel(sourcePixels, y, x, this.imageWidth, this.imageHeight);
                        bottomTriangleColors.Add(color);
                    }

                    currentX++;
                }

                currentY++;
            }

            this.colorTriangle(sourcePixels, topTriangleColors, topTriangleCoordinates, isBlackAndWhite);

            this.colorTriangle(sourcePixels, bottomTriangleColors, bottomTriangleCoordinates, isBlackAndWhite);
        }

        private void colorTriangle(byte[] sourcePixels, List<Color> triangleColors,
            List<Tuple<int, int>> triangleCoordinates, bool isBlackAndWhite)
        {
            if (isBlackAndWhite)
            {
                var newColor = this.createBlackAndWhiteTriangles(triangleColors);
                foreach (var coordinate in triangleCoordinates)
                {
                    ImagePixel.SetPixel(sourcePixels, coordinate.Item2, coordinate.Item1, newColor,
                        this.imageWidth, this.imageHeight);
                }
            }
            else
            {
                var newColor = this.createColorTriangles(triangleColors);
                foreach (var coordinate in triangleCoordinates)
                {
                    ImagePixel.SetPixel(sourcePixels, coordinate.Item2, coordinate.Item1, newColor,
                        this.imageWidth, this.imageHeight);
                }
            }
        }

        private Color createColorTriangles(List<Color> triangleColors)
        {
            var totalRed = 0;
            var totalBlue = 0;
            var totalGreen = 0;
            foreach (var currentColor in triangleColors)
            {
                totalRed += currentColor.R;
                totalBlue += currentColor.B;
                totalGreen += currentColor.G;
            }

            var averageTopRed = (byte) (totalRed / triangleColors.Count);
            var averageTopBlue = (byte) (totalBlue / triangleColors.Count);
            var averageTopGreen = (byte) (totalGreen / triangleColors.Count);
            var newColor = new Color {
                R = averageTopRed,
                G = averageTopGreen,
                B = averageTopBlue
            };
            return newColor;
        }

        private Color createBlackAndWhiteTriangles(List<Color> triangleColors)
        {
            var sumOfColors = 0;
            foreach (var currentColor in triangleColors)
            {
                sumOfColors += currentColor.R + currentColor.B + currentColor.G;
            }

            var averageAllColors = sumOfColors / 3;
            var newColor = new Color();
            if (averageAllColors >= 127.5)
            {
                newColor.R = 255;
                newColor.G = 255;
                newColor.B = 255;
            }
            else
            {
                newColor.R = 0;
                newColor.G = 0;
                newColor.B = 0;
            }

            return newColor;
        }

        #endregion
    }
}