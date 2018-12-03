using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    public class FolderImage
    {
        public WriteableBitmap imageBitmap { get; private set; }

        public String FileName { get; private set; }

        public FolderImage(WriteableBitmap loadedBitmap, string fileName)
        {
            this.imageBitmap = loadedBitmap;
            this.FileName = fileName;
        }

        /// <summary>
        /// Finds the average color for the stored image
        /// </summary>
        /// <returns></returns>
        public Color FindAverageColor()
        {
            var imageWidth = (uint) this.imageBitmap.PixelWidth;

            var imageHeight = (uint) this.imageBitmap.PixelHeight;

            var sourcePixels = this.imageBitmap.PixelBuffer.ToArray();

            var averageColor = ImageAverageColor.FindAverageColorOfEntireImage(sourcePixels, imageWidth, imageHeight);

            return averageColor;
        }


        /// <summary>
        /// Resizes the writable bitmap.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>Nothing</returns>
        public async Task ResizeWritableBitmap( uint width, uint height)
        {

            Stream stream = this.imageBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);

            var inMemoryRandomStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height, 96, 96, pixels);
            await encoder.FlushAsync();
     

            var transform = new BitmapTransform
            {
                ScaledWidth = width,
                ScaledHeight = height
            };
            inMemoryRandomStream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(inMemoryRandomStream);
            var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Rgba8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage);

            var sourceDecodedPixels = pixelData.DetachPixelData();
           

            var inMemoryRandomStream2 = new InMemoryRandomAccessStream();
            var encoder2 = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream2);
            encoder2.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height, 96, 96, sourceDecodedPixels);
            await encoder2.FlushAsync();
            inMemoryRandomStream2.Seek(0);

            var bitmap = new WriteableBitmap((int)width, (int)height);
            await bitmap.SetSourceAsync(inMemoryRandomStream2);
            this.imageBitmap = bitmap;
        }


        public override string ToString()
        {

            return this.FileName;
        }




    }
}
