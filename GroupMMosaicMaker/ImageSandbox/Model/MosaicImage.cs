using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly uint imageHeight;
        private readonly uint imageWidth;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MosaicImage"/> class.
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

        private async Task<BitmapImage> CreateAImageFromFile(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        /// <summary>
        /// Creates the solid mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        public void CreateSolidMosaic(byte[] sourcePixels)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight , y);

                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);
                
                    this.setNewColorValue(sourcePixels, y, yStoppingPoint, x, xStoppingPoint, false);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }
        /// <summary>
        /// Creates the triangle mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="isBlackAndWhite">if set to <c>true</c> [is black and white].</param>
        public void CreateTriangleMosaic(byte[] sourcePixels, bool isBlackAndWhite)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                

                while (x < imageWidth)
                {
                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    this.TriangleMosaic(sourcePixels, x, y, xStoppingPoint, yStoppingPoint, isBlackAndWhite);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }


        /// <summary>
        /// Creates the picture mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="loadedImages">The loaded images.</param>
        public void CreatePictureMosaic(byte[] sourcePixels, ImagePalette loadedImages)
        {
            for (var y = 0; y < imageHeight; y += this.BlockSize)
            {
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                for (var x = 0; x < imageWidth; x += this.BlockSize)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    this.setPictureMosaic(sourcePixels, y, yStoppingPoint, x,
                        xStoppingPoint, loadedImages);
                }
            }
        }

        /// <summary>
        /// Finds the triangle points.
        /// </summary>
        /// <returns>A list of coordinates where the triangle occurs</returns>
        public List<Tuple<int, int>> FindTrianglePoints()
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

        private void setPictureMosaic(byte[] sourcePixels,int startingYPoint, int yStoppingPoint,
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

        private void setNewColorValue(byte[] sourcePixels, int startingYPoint,
            int yStoppingPoint, int startingXPoint, int xStoppingPoint, bool isBlackAndWhite)
        {

            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);
            var averageBlack = ImageAverageColor.FindAverageBlackAndWhiteColorForSelectedArea(sourcePixels, imageWidth,
                imageHeight, startingYPoint,
                yStoppingPoint, startingXPoint, xStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);

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
        /// Creates the black and white mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        public void CreateBlackAndWhiteMosaic(byte[] sourcePixels)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                    this.setNewColorValue(sourcePixels, y, yStoppingPoint, x, xStoppingPoint,
                        true);

                    x += this.BlockSize;
                }

                y += this.BlockSize;
            }
        }

        private void TriangleMosaic(byte[] sourcePixels, int xStart, int yStart,
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
                    if (currentY < currentX)
                    {
                        topTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, y, x, imageWidth, imageHeight);
                        topTriangleColors.Add(color);
                    }
                    else
                    {
                        bottomTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, y, x, imageWidth, imageHeight);
                        bottomTriangleColors.Add(color);
                    }

                    currentX++;
                }

                currentY++;
            }
            this.colorTriangle(sourcePixels, topTriangleColors, topTriangleCoordinates, isBlackAndWhite);
           
            this.colorTriangle(sourcePixels, bottomTriangleColors, bottomTriangleCoordinates, isBlackAndWhite);
        }

        private void colorTriangle(byte[] sourcePixels,List<Color> triangleColors, List<Tuple<int, int>> triangleCoordinates, bool isBlackAndWhite)
        {
            if (isBlackAndWhite)
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
                    newColor.R =0;
                    newColor.G = 0;
                    newColor.B = 0;
                }
                foreach (var coordinate in triangleCoordinates)
                {
                    ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item2, coordinate.Item1, newColor, imageWidth,
                        imageHeight);
                }
            }
            else
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

                var averageTopRed = (byte)(totalRed / triangleColors.Count);
                var averageTopBlue = (byte)(totalBlue / triangleColors.Count);
                var averageTopGreen = (byte)(totalGreen / triangleColors.Count);
                var newColor = new Color();
                newColor.R = averageTopRed;
                newColor.G = averageTopGreen;
                newColor.B = averageTopBlue;
                foreach (var coordinate in triangleCoordinates)
                {
                    ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item2, coordinate.Item1, newColor, imageWidth,
                        imageHeight);
                }
            }
            
        }

        #endregion
        
    }
}