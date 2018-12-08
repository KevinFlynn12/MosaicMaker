using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml;
using Windows.Globalization.DateTimeFormatting;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.UI;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    internal class MosaicImage
    {
        #region Data members

        //private byte[] sourcePixels;

        #endregion

        #region Properties

        public bool HasGrid { get; set; }

        public StorageFile ImageFile { get; set; }

        #endregion

        #region Constructors

        public MosaicImage(bool hasGrid, StorageFile imageFile)
        {
            this.HasGrid = hasGrid;
            this.ImageFile = imageFile;
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

        public void CreateSolidMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize,
            bool isGrid)
        {
            var y = 0;
            var iterationX = 0;
            var iterationY = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth-1, x, blockSize);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight-1, y, blockSize);

                    this.TopTriangle(sourcePixels, imageWidth, imageHeight, x, y, xStoppingPoint, yStoppingPoint, iterationX, iterationY, blockSize);
                    //this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                      //  isGrid, false, blockSize);

                    x += blockSize;
                    iterationX++;
                }
                y += blockSize;
                iterationY++;
            }
            
        }

        public void CreatePictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize,
            FolderImageRegistry loadedImages)
        {

            for(int y=0; y < imageHeight ; y+=blockSize)
            {
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                for (int x = 0; x < imageWidth; x += blockSize)
                {
                    

                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);


                    this.setPictureMosaic(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x,
                        xStoppingPoint, false, loadedImages);

                }

            }
        }


        public List<Tuple<int, int>> FindTrianglePoints(uint imageWidth, uint imageHeight, int blockSize)
        {
            int iterations;
            if (imageWidth % blockSize == 0)
            {
                iterations = (int)imageWidth / blockSize;
            }
            else
            {
                iterations = (int)imageWidth / blockSize + 1;
            }

            var triangleCoordinates = new List<Tuple<int, int>>();

            for (var x = 0; x <= imageHeight; x += blockSize)
            {
                for (var y = 0; y <= imageHeight; y += blockSize)
                {
                    for (var currentXPoint = x;
                        currentXPoint < this.UpdateStoppingPoint(imageWidth, x, blockSize);
                        currentXPoint++)
                    {
                        for (var currentYPoint = y;
                            currentYPoint < this.UpdateStoppingPoint(imageHeight, y, blockSize);
                            currentYPoint++)
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                if (currentYPoint == currentXPoint + (blockSize * i) ||
                                    currentXPoint == currentYPoint + (blockSize * i))
                                {
                                    var coordinate = new Tuple<int, int>(currentXPoint, currentYPoint);
                                    triangleCoordinates.Add(coordinate);

                                }
                            }
                        }
                    }
                }
            }

            return triangleCoordinates;

        }
        private void setPictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            int startingYPoint, int yStoppingPoint,
            int startingXPoint, int xStoppingPoint, bool isBlackAndWhite, FolderImageRegistry loadedImages)
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
                            imageHeight, isBlackAndWhite);


                        matchingImageX++;
                    }

                    matchingImageY++;
                }
            
        }


        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint,
            int yStoppingPoint, int startingXPoint, int xStoppingPoint, bool isGrid, bool isBlackAndWhite, int blockSize)
        {
            int iterations;
            if (imageWidth % blockSize == 0)
            {
                iterations = (int)imageWidth / blockSize;
            }
            else
            {
                iterations = (int)imageWidth / blockSize+1;
            }
            
            
            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);
                    
                    if (isGrid)
                    {
                        
                        if (currentYPoint == startingYPoint || yStoppingPoint == currentYPoint
                                                            || currentXPoint == startingXPoint ||
                                                            xStoppingPoint == currentXPoint)
                        {
                            pixelColor = Colors.White;
                           
                        }
                        
                        else if (isBlackAndWhite)
                        {
                            var averageBlack = averageColor.R + averageColor.B + averageColor.G / 3;
                            pixelColor.R = (byte) averageBlack;
                            pixelColor.B = (byte) averageBlack;
                            pixelColor.G = (byte) averageBlack;
                        }
                        else
                        {
                            pixelColor.R = averageColor.R;
                            pixelColor.B = averageColor.B;
                            pixelColor.G = averageColor.G;
                        }
                        for (int i = 0; i < iterations; i++)
                        {
                            if (currentYPoint == currentXPoint + (40 * i) ||
                                currentXPoint == currentYPoint + (40 * i))
                            {
                                pixelColor = Colors.White;
                            }
                        }
                    }
                    else if (isBlackAndWhite & !isGrid)
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
                        imageHeight, isBlackAndWhite);
                }
                
            }
        }

        

        private int UpdateStoppingPoint(uint maxValue, int coordinate, int blockSize)
        {
            var coordinateStoppingPoint = coordinate + blockSize;
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
        public void CreateBlackAndWhiteMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize,
            bool isGrid)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                        isGrid, true,blockSize);

                    x += blockSize;
                }

                y += blockSize;
            }
        }



        private void TopTriangle(byte[] sourcePixels, uint imageHeight, uint imageWidth, int xStart, int yStart, int xStopping, int yStoppingPoint, int iterationX, int iterationY, int blockSize)
        {
            var topTriangleColors = new List<Color>();
            var topTriangleCoordinates = new List<Tuple<int, int>>();
            for (int y = yStoppingPoint; y > yStart; y--)
            {
                for (int x = xStopping; x > xStart; x--)
                {
                    if (-y + blockSize * iterationY < -x + blockSize * iterationX)
                    {
                        topTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, x, y, imageWidth, imageHeight);
                        topTriangleColors.Add(color);

                    }
                    
                }
            }

            var totalRed = 0;
            var totalBlue = 0;
            var totalGreen = 0;
            foreach (var currentColor in topTriangleColors)
            {
                totalRed += currentColor.R;
                totalBlue += currentColor.B;
                totalGreen += currentColor.G;

            }

            var averageRed = (byte) (totalRed / topTriangleColors.Count);
            var averageBlue = (byte)(totalBlue / topTriangleColors.Count);
            var averageGreen = (byte)(totalGreen / topTriangleColors.Count);
            var newColor = new Color();
            newColor.R = averageRed;
            newColor.G = averageGreen;
            newColor.B = averageBlue;
            foreach (var coordinate in topTriangleCoordinates)
            {    
                ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item1, coordinate.Item2, newColor, imageWidth, imageHeight, false);
            }
        }

        #endregion
    }
}