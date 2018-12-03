using ImageSandbox.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Model
{
    class MosaicImage
    {
        public bool HasGrid { get; set; }

        public StorageFile ImageFile { get; set; }
        
        private byte[] sourcePixels;

        public MosaicImage(bool hasGrid, StorageFile imageFile)
        {
            this.HasGrid = hasGrid;
            this.ImageFile = imageFile;
        }

        private async void createSourcePixels()
        {
            var imageTask = this.CreateAImageFromFile(this.ImageFile);
            var image = imageTask.Result;
            using (var fileStream = await this.ImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform
                {
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

               this.sourcePixels = pixelData.DetachPixelData();
                
            }
        }
        private async Task<BitmapImage> CreateAImageFromFile(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
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

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, YStoppingPoint, x, XStoppingPoint, isGrid, false);

                    x += blockSize;
                }
                y += blockSize;
            }
        }

        public async Task CreatePictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize, FolderImageRegistry loadedImages)
        {
          await loadedImages.ResizeAllImagesInFolder((uint) blockSize, (uint) blockSize);

            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                   await  this.setPictureMosaic(sourcePixels, imageWidth, imageHeight, y, YStoppingPoint, x, XStoppingPoint, false,  loadedImages, blockSize);

                    x += blockSize;
                }
                y += blockSize;
            }
        }


        private async Task setPictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            int startingYPoint, int YStoppingPoint, 
            int startingXPoint, int XStoppingPoint, bool isBlackAndWhite, FolderImageRegistry loadedImages, int blockSize)
        {
            var count = loadedImages.Count;

            var averageColor =
               ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    YStoppingPoint, startingXPoint, XStoppingPoint);

            var matchingImage = loadedImages.FindClosestMatchingImage(averageColor);

            int matchingImageY = 0;
            var pixelColor = Colors.White;
            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                int matchingImageX = 0;
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    if (!(currentYPoint == startingYPoint || YStoppingPoint == currentYPoint
                                                                || currentXPoint == startingXPoint || XStoppingPoint == currentXPoint))
                    {
                        pixelColor = Colors.White;
                        ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight, isBlackAndWhite);

                    }
                    pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);

                    pixelColor = this.getMatchingImagePixel(matchingImage, matchingImageX, matchingImageY);

                    ImagePixel.setPixelBgra8(sourcePixels,currentXPoint,currentYPoint, pixelColor,imageWidth, imageHeight, isBlackAndWhite);

                    matchingImageX++;
                }

                matchingImageY++;
            }

        }





        private Color getMatchingImagePixel(FolderImage matchingImage, int x, int y)
        {
            var imageWidth = (uint) matchingImage.imageBitmap.PixelWidth;

            var pixelColor = new Color();

            var imageHeight = (uint) matchingImage.imageBitmap.PixelHeight;

            var sourcePixels = matchingImage.imageBitmap.PixelBuffer.ToArray();

            for (int currentY = 0; currentY < imageHeight; currentY++)
            {
                for (int currentX = 0; currentX < imageWidth; currentX++)
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



        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint, int XStoppingPoint, bool isGrid, bool isBlackAndWhite)
        {
            var averageColor =
                 ImageAverageColor.FindAverageColorForSelectedArea(sourcePixels, imageWidth, imageHeight, startingYPoint,
                    YStoppingPoint, startingXPoint, XStoppingPoint);
            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);

                    if (isGrid)
                    {
                        if (currentYPoint == startingYPoint || YStoppingPoint == currentYPoint
                                                || currentXPoint == startingXPoint || XStoppingPoint == currentXPoint)
                        {
                            pixelColor = Colors.White;

                        }else if (isBlackAndWhite)
                        {
                            var averageBlack = averageColor.R + averageColor.B + averageColor.G / 3;
                            pixelColor.R = (byte)averageBlack;
                            pixelColor.B = (byte)averageBlack;
                            pixelColor.G = (byte)averageBlack;
                        }
                        else
                        {
                            pixelColor.R = averageColor.R;
                            pixelColor.B = averageColor.B;
                            pixelColor.G = averageColor.G;
                        }
                    }else if (isBlackAndWhite & !isGrid)
                    {
                        var averageBlack = averageColor.R + averageColor.B + averageColor.G / 3;
                        pixelColor.R = (byte) averageBlack;
                        pixelColor.B = (byte)averageBlack;
                        pixelColor.G = (byte)averageBlack;
                    }
                    else
                    {
                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;
                    }


                    ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight, isBlackAndWhite);
                }
            }
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



        /*
         Issue the graph y=x+b grows faster than y=x*cos(45) (y=x*1/sqrt(2)) therefore once y=x  exceeds that of y=x*cos(45) the for loops will not run
        public Color FindAverageColorOfTriangleMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
            int XStoppingPoint)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;
            var stoppingYPoint = YStoppingPoint * Math.Cos(45);
            var stoppingXPoint = XStoppingPoint * Math.Sin(45);
            for (var currentYPoint = startingYPoint; currentYPoint < stoppingYPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < stoppingXPoint; currentXPoint++)
                {
                    pixelCount++;
                    var pixelColor = this.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);
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
        */


        //remove
        public Color FindAverageColor(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
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
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);
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


        //remove
        public Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        //remove
        public void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height, bool isBlackAndWhite)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;

        }



        /// <summary>
        /// Creates the black and white mosaic.
        /// </summary>
        /// <param name="sourcePixels">The source pixels.</param>
        /// <param name="imageWidth">Width of the image.</param>
        /// <param name="imageHeight">Height of the image.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="isGrid">if set to <c>true</c> [is grid].</param>
        public void CreateBlackAndWhiteMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight, int blockSize, bool isGrid)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, x, blockSize);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, y, blockSize);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, YStoppingPoint, x, XStoppingPoint, isGrid, true);

                    x += blockSize;
                }
                y += blockSize;
            }
        }
    }
}


