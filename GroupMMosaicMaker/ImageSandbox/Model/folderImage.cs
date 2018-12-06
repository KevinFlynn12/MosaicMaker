using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;
using Windows.Foundation;

namespace ImageSandbox.Model
{
    public class FolderImage
    {
        private WriteableBitmap fileWriteableBitmap;
        private string name;
        private IAsyncOperation<StorageItemThumbnail> thumbnail;
        #region Properties

        public WriteableBitmap ImageBitmap { get; private set; }

        public string FileName { get; }

        public IAsyncOperation<StorageItemThumbnail> ThumbNail { get; private set; }

        #endregion

        #region Constructors

        public FolderImage(WriteableBitmap loadedBitmap, string fileName, IAsyncOperation<StorageItemThumbnail> pictureThumbNail)
        {
            this.ThumbNail = pictureThumbNail;
            this.ImageBitmap = loadedBitmap;
            this.FileName = fileName;
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
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Nothing</returns>
        public async Task ResizeWritableBitmap(uint width, uint height)
        {
                       
            
            
        }

        public override string ToString()
        {
            return this.FileName;
        }

        #endregion
    }
}