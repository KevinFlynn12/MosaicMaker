using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    public class FolderImage
    {
        public WriteableBitmap imageBitmap { get; private set; }


        public FolderImage(WriteableBitmap loadedBitmap)
        {
            this.imageBitmap = loadedBitmap;
        }

        public Color FindAverageColor()
        {
            var imageWidth = (uint) this.imageBitmap.PixelWidth;

            var imageHeight = (uint) this.imageBitmap.PixelHeight;

            var sourcePixels = this.imageBitmap.PixelBuffer.ToArray();

            var averageColor = ImageAverageColor.FindAverageColorOfEntireImage(sourcePixels, imageWidth, imageHeight);

            return averageColor;
        }

    }
}
