﻿using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    public class FolderImage
    {
        #region Data members
        
        private readonly StorageFile loadedImageFile;

        #endregion

        #region Properties

        public WriteableBitmap ImageBitmap { get; private set; }

        public string FileName { get; }

        public IAsyncOperation<StorageItemThumbnail> ThumbNail { get; private set; }

        #endregion

        #region Constructors

        public FolderImage(WriteableBitmap loadedBitmap, StorageFile loadedFile)
        {
            this.ImageBitmap = loadedBitmap;
            this.loadedImageFile = loadedFile;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Finds the average color for the stored image
        /// </summary>
        /// <returns></returns>
        public Color FindAverageColor()
        {
            var imageWidth = (uint) this.ImageBitmap.PixelWidth;

            var imageHeight = (uint) this.ImageBitmap.PixelHeight;

            var sourcePixels = this.ImageBitmap.PixelBuffer.ToArray();

            var averageColor = ImageAverageColor.FindAverageColorOfEntireImage(sourcePixels, imageWidth, imageHeight);

            return averageColor;
        }

        /// <summary>
        ///     Resizes the writable bitmap.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <returns>Nothing</returns>
        public async Task ResizeWritableBitmap(int blockSize)
        {
            using (var fileStream = await this.loadedImageFile.OpenAsync(FileAccessMode.Read))

            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform {
                    ScaledWidth = Convert.ToUInt32(blockSize),
                    ScaledHeight = Convert.ToUInt32(blockSize)
                };

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                var fileWriteableBitmap =
                    new WriteableBitmap((int) transform.ScaledWidth, (int) transform.ScaledHeight);

                using (var writeStream = fileWriteableBitmap.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);

                    this.ImageBitmap = fileWriteableBitmap;
                }
            }
        }

        public override string ToString()
        {
            return this.FileName;
        }

        #endregion
    }
}