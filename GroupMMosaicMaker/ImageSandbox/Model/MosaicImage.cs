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
                var yStoppingPoint = this.UpdateStoppingPoint(imageHeight-1, y, blockSize);

                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth-1, x, blockSize);


                    this.TriangleMosaic(sourcePixels, imageWidth, imageHeight, x, y, xStoppingPoint, yStoppingPoint, iterationX, iterationY, blockSize);
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
            if (imageWidth > imageHeight)
            {
                if (imageWidth % blockSize == 0)
                {
                    iterations = (int)imageWidth / blockSize;
                }
                else
                {
                    iterations = (int)imageWidth / blockSize + 1;
                }
            }
            else
            {
                if (imageHeight % blockSize == 0)
                {
                    iterations = (int)imageHeight / blockSize;
                }
                else
                {
                    iterations = (int)imageHeight / blockSize + 1;
                }
            }

            
            var triangleCoordinates = new List<Tuple<int, int>>();

            for (var x = 0; x <= imageHeight; x += blockSize)
            {
                for (var y = 0; y <= imageHeight; y += blockSize)
                {
                    var currentX = 0;
                    for (var currentXPoint = x;
                        currentXPoint < this.UpdateStoppingPoint(imageWidth, x, blockSize);
                        currentXPoint++)
                    {
                        
                        var currentY = 0;
                        for (var currentYPoint = y;
                            currentYPoint < this.UpdateStoppingPoint(imageHeight, y, blockSize);
                            currentYPoint++)
                        {
                            if (currentYPoint == y || this.UpdateStoppingPoint(imageHeight, y, blockSize) == currentYPoint
                                                                || currentXPoint == x ||
                                                   this.UpdateStoppingPoint(imageWidth, x, blockSize) == currentXPoint)
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
                            /*
                            for (int i = 0; i <= iterations*2; i++)
                            {
                                if ( currentYPoint == currentXPoint + (blockSize * i) ||
                                    currentXPoint == currentYPoint + (blockSize * i) || currentYPoint + blockSize*i == currentXPoint + blockSize*i || currentYPoint== currentXPoint - (blockSize * i)||
                                     currentXPoint == currentYPoint - (blockSize * i))
                                {
                                    var coordinate = new Tuple<int, int>(currentXPoint, currentYPoint);
                                    triangleCoordinates.Add(coordinate);

                                }
                            }
                            */
                        }
                        currentX++;
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



        private void TriangleMosaic(byte[] sourcePixels, uint imageHeight, uint imageWidth, int xStart, int yStart, int xStopping, int yStoppingPoint, int iterationX, int iterationY, int blockSize)
        {
            var topTriangleColors = new List<Color>();
            var topTriangleCoordinates = new List<Tuple<int, int>>();
            var bottomTriangleColors = new List<Color>();
            var bottomTriangleCoordinates = new List<Tuple<int, int>>();
            var currentY = 0;
            for (int y = yStart;  y < yStoppingPoint; y++)
            {
                var currentX = 0;
                for (int x = xStart; x < xStopping ; x++)
                {
                    if (currentY <currentX )
                    {
                        topTriangleCoordinates.Add(new Tuple<int, int>(x, y));
                        var color = new Color();
                        color = ImagePixel.GetPixelBgra8(sourcePixels, x, y, imageWidth, imageHeight);
                        topTriangleColors.Add(color);
                    }else if (currentY >= currentX)
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
            var averageTopBlue = (byte)(topTotalBlue / topTriangleColors.Count);
            var averageTopGreen = (byte)(topTotalGreen / topTriangleColors.Count);
            var newColor = new Color();
            newColor.R = averageTopRed;
            newColor.G = averageTopGreen;
            newColor.B = averageTopBlue;
            foreach (var coordinate in topTriangleCoordinates)
            {    
                ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item1, coordinate.Item2, newColor, imageWidth, imageHeight, false);
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

            var bottomAverageTopRed = (byte)(bottomTotalRed / bottomTriangleColors.Count);
            var bottomAverageTopBlue = (byte)(bottomTotalBlue / bottomTriangleColors.Count);
            var bottomAverageTopGreen = (byte)(bottomTotalGreen / bottomTriangleColors.Count);
            var newBottomColor = new Color();
            newBottomColor.R = bottomAverageTopRed;
            newBottomColor.G = bottomAverageTopGreen;
            newBottomColor.B = bottomAverageTopBlue;
            foreach (var coordinate in bottomTriangleCoordinates)
            {
                ImagePixel.setPixelBgra8(sourcePixels, coordinate.Item1, coordinate.Item2, newBottomColor, imageWidth, imageHeight, false);
            }
        }

        #endregion
    }
}