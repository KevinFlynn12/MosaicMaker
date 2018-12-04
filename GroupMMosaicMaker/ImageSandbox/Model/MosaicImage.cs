using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
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
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x, xStoppingPoint,
                        isGrid, false, blockSize);

                    x += blockSize;
                }
                y += blockSize;
                multiplicity++;
            }
            
        }

        public async Task CreatePictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize,
            FolderImageRegistry loadedImages)
        {

            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var xStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var yStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    if (x == 300)
                    {
                        var donothing = 0;
                    }


                    this.setPictureMosaic(sourcePixels, imageWidth, imageHeight, y, yStoppingPoint, x,
                        xStoppingPoint, false, loadedImages);

                    x += blockSize;
                }

                y += blockSize;
            }
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
            
            for (var currentXPoint = startingXPoint; currentXPoint < xStoppingPoint; currentXPoint++)
            {
                

                var matchingImageX = 0;
                for (var currentYPoint = startingYPoint; currentYPoint < yStoppingPoint; currentYPoint++)
                {
                    

                    Color pixelColor;
                    if ((currentYPoint == startingYPoint || yStoppingPoint == currentYPoint
                                                          || currentXPoint == startingXPoint ||
                                                          xStoppingPoint == currentXPoint))
                    {
                        pixelColor = Colors.White;
                        ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth,
                            imageHeight, isBlackAndWhite);
                    }

                    if (!(currentYPoint == startingYPoint || yStoppingPoint == currentYPoint
                                                         || currentXPoint == startingXPoint ||
                                                         xStoppingPoint == currentXPoint))
                    {
                        pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                            imageHeight);



                        //pixelColor = this.getMatchingImagePixel(matchingImage, matchingImageX, matchingImageY);

                        pixelColor = matchingImage.ImageBitmap.GetPixel(matchingImageX, matchingImageY);

                        ImagePixel.setPixelBgra8(sourcePixels, currentXPoint, currentYPoint, pixelColor, imageWidth,
                            imageHeight, isBlackAndWhite);
                    }


                    /*
                    pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth,
                        imageHeight);



                    //pixelColor = this.getMatchingImagePixel(matchingImage, matchingImageX, matchingImageY);

                    pixelColor = matchingImage.ImageBitmap.GetPixel(matchingImageX, matchingImageY);

                    ImagePixel.setPixelBgra8(sourcePixels, currentXPoint, currentYPoint, pixelColor, imageWidth,
                        imageHeight, isBlackAndWhite);
                    */
                    matchingImageX++;
                }

                matchingImageY++;
            }
        }

        private Color getMatchingImagePixel(FolderImage matchingImage, int x, int y)
        {
            var imageWidth = (uint) matchingImage.ImageBitmap.PixelWidth;

            var pixelColor = new Color();

            var imageHeight = (uint) matchingImage.ImageBitmap.PixelHeight;

            var sourcePixels = matchingImage.ImageBitmap.PixelBuffer.ToArray();

            for (var currentY = 0; currentY < imageHeight; currentY++)
            {
                for (var currentX = 0; currentX < imageWidth; currentX++)
                {
                    if (currentX == x && currentY == y)
                    {
                        pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, y, x, imageWidth, imageHeight);

                        return pixelColor;
                    }
                }
            }

            return pixelColor;
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

        #endregion
    }
}