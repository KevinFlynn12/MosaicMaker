using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Media.Imaging;
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

        /*
        private async void createSourcePixels()
        {
            var imageTask = this.CreateAImageFromFile(this.ImageFile);
            var image = imageTask.Result;
            using (var fileStream = await this.ImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(image.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(image.PixelHeight)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

               // this.sourcePixels = pixelData.DetachPixelData();
            }
        }
        */
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
            var multiplicity = 0;
            var triangleCoordinates = this.FindTrianglePoints(imageWidth, imageHeight, blockSize);
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                        isGrid, false, blockSize, multiplicity);

                    x += blockSize;
                }

                y += blockSize;
                multiplicity++;
            }
            foreach (var currentPoint in triangleCoordinates)
            {
                var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1, imageWidth,imageHeight);
                pixelColor = Colors.White;
                ImagePixel.setPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1, pixelColor, imageWidth,
                    imageHeight, false);
            }

        }

        public async Task CreatePictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize,
            FolderImageRegistry loadedImages)
        {

            for (int y = 0; y < imageHeight; y += blockSize)
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

        private List<Tuple<int, int>> FindTrianglePoints(uint imageWidth, uint imageHeight, int blockSize)
        {
            int iterations;
            if (imageWidth % blockSize == 0)
            {
                iterations = (int) imageWidth / blockSize;
            }
            else
            {
                iterations = (int) imageWidth / blockSize + 1;
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

        public Color FindAverageColorForBottomTriangle(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            int startingYPoint, int YStoppingPoint, int startingXPoint,
            int XStoppingPoint, int blockSize, int iteration, bool isBottom)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;
            var triangleCoordinates = FindTrianglePoints(imageWidth, imageHeight, blockSize);
            var bottomPoints = new List<Tuple<int, int>>();
            var topPoints = new List<Tuple<int, int>>();
            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {

                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    if (!triangleCoordinates.Contains(new Tuple<int, int>(currentXPoint, currentYPoint)))
                    {
                        if (isBottom)
                        {
                            if (-currentYPoint + (blockSize * iteration) >= -currentXPoint + (blockSize * iteration) )
                            {
                                bottomPoints.Add(new Tuple<int, int>(currentXPoint, currentYPoint));
                            }else if (currentYPoint >= currentXPoint && -currentXPoint + blockSize * iteration > 0)
                            {
                                bottomPoints.Add(new Tuple<int, int>(currentXPoint, currentYPoint));
                            }
                        }
                        else
                        {
                            if (currentYPoint + (blockSize * iteration) < -currentXPoint + (blockSize * iteration))
                            {
                                topPoints.Add(new Tuple<int, int>(currentXPoint, currentYPoint));
                            }
                            else if (currentYPoint <= currentXPoint && -currentXPoint + blockSize * iteration > 0)
                            {
                                topPoints.Add(new Tuple<int, int>(currentXPoint, currentYPoint));
                            }
                        }
                    }
                }
            }

            if (isBottom)
            {
                foreach (var points in bottomPoints)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, points.Item2, points.Item1, imageWidth,
                        imageHeight);
                    totalRed += pixelColor.R;
                    totalBlue += pixelColor.B;
                    totalGreen += pixelColor.G;
                }
            }
            else
            {
                foreach (var points in topPoints)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, points.Item2, points.Item1, imageWidth,
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
            if (isBottom)
            {
                foreach (var points in bottomPoints)
                {
                    pixelCount++;
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, points.Item2, points.Item1, imageWidth,
                        imageHeight);
                    pixelColor.R = newColor.R;
                    pixelColor.B = newColor.B;
                    pixelColor.G = newColor.G;
                    ImagePixel.setPixelBgra8(sourcePixels, points.Item2, points.Item1, pixelColor, imageWidth,
                        imageHeight, false);
                }
            }
            else
            {
                foreach (var points in topPoints)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, points.Item2, points.Item1, imageWidth,
                        imageHeight);
                    pixelColor.R = 0;//newColor.R;
                    pixelColor.B = 0;// newColor.B;
                    pixelColor.G =0;// newColor.G;
                    ImagePixel.setPixelBgra8(sourcePixels, points.Item2, points.Item1, pixelColor, imageWidth,
                        imageHeight,false);
                }
            }
        
            return newColor;
            
        }



        /*
        for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
        {

            for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
            {
                if (isBottom)
                {

                    if (currentYPoint <= -(currentXPoint - blockSize * iteration) || currentXPoint >= -(currentYPoint - blockSize * iteration))
                    {
                        pixelCount++;
                        var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                            imageHeight);
                        totalRed += pixelColor.R;
                        totalBlue += pixelColor.B;
                        totalGreen += pixelColor.G;
                    }
                }
                else
                {
                    if (currentYPoint >= -(currentXPoint -blockSize * iteration) || currentXPoint <= -(currentYPoint - blockSize * iteration)) 
                    {
                        pixelCount++;
                        var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                            imageHeight);
                        totalRed += pixelColor.R;
                        totalBlue += pixelColor.B;
                        totalGreen += pixelColor.G;
                    }
                }

                */







        private async void setPictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight,
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
                    /*
                    if ((currentYPoint == startingYPoint || yStoppingPoint == currentYPoint
                                                         || currentXPoint == startingXPoint ||
                                                         xStoppingPoint == currentXPoint))
                    {
                       pixelColor = Colors.White;
                       ImagePixel.setPixelBgra8(sourcePixels,  currentXPoint, currentYPoint, pixelColor, imageWidth,
                       imageHeight, isBlackAndWhite);
                    }
                    */

                    pixelColor = matchingImage.ImageBitmap.GetPixel(matchingImageX, matchingImageY);

                    //coordinates have been swapped
                    ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                        imageHeight, isBlackAndWhite);


                    matchingImageX++;
                }

                matchingImageY++;
            }
        }


        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint,
            int yStoppingPoint, int startingXPoint, int xStoppingPoint, bool isGrid, bool isBlackAndWhite,
            int blockSize, int iteration)
        {


            var averageColor =
                ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    yStoppingPoint, startingXPoint, xStoppingPoint);
            var averageColorBottom = FindAverageColorForBottomTriangle(sourcePixels, imageWidth, imageHeight,
                startingYPoint,
                yStoppingPoint, startingXPoint, xStoppingPoint, blockSize, iteration, true);
           var averageColorTop = FindAverageColorForBottomTriangle(sourcePixels, imageWidth, imageHeight,
                startingYPoint,
                yStoppingPoint, startingXPoint, xStoppingPoint, blockSize, iteration, false);
            var triangleCoordinates = FindTrianglePoints(imageWidth, imageHeight, blockSize);
            var bottomPoints = triangleCoordinates.Where(coordinate =>
                coordinate.Item1 >= startingYPoint &&
                coordinate.Item1 <= yStoppingPoint &&
                coordinate.Item2 >= coordinate.Item1).ToList();
            var topPoints = triangleCoordinates.Where(coordinate =>
                coordinate.Item1 >= startingYPoint &&
                coordinate.Item1 <= xStoppingPoint &&
                coordinate.Item2 >= coordinate.Item1).ToList();
            /*
            foreach (var point in bottomPoints)
            {
                var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, point.Item2, point.Item1, imageWidth,
                    imageHeight);
                pixelColor.R = averageColorBottom.R;
                pixelColor.B = averageColorBottom.B;
                pixelColor.G = averageColorBottom.G;
                ImagePixel.setPixelBgra8(sourcePixels, point.Item2, point.Item1, pixelColor, imageWidth,
                    imageHeight, isBlackAndWhite);
            }

            *
            for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);


                    /*
                    if (currentYPoint <= -currentXPoint + blockSize * iteration || currentXPoint < -currentYPoint + blockSize * iteration)
                    {
                        pixelColor.R = 0;//averageColorBottom.R;
                        pixelColor.B = 0;// averageColorBottom.B;
                        pixelColor.G = 0;//averageColorBottom.G;
                        ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                            imageHeight, isBlackAndWhite);
                    }

                    
                    else if (currentYPoint >= -(currentXPoint - blockSize * iteration)  || currentXPoint >= -currentYPoint +  blockSize * iteration )

                    {
                        pixelColor.R = 255;//averageColorTop.R;
                        pixelColor.B = 255;//averageColorTop.B;
                        pixelColor.G = 255;//averageColorTop.G;
                        ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                            imageHeight, isBlackAndWhite);
                    }
                    *
                }
            }
            */
        }
        /*
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


        }
        else if (isBlackAndWhite & !isGrid)
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
        */





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
                    /*
                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                     isGrid, true);
                     */
                    x += blockSize;
                }

                y += blockSize;
            }
        }

        #endregion
    }
}