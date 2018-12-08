using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Util
{
    public static class CompareBItMaps
    {
        public static bool Equals(WriteableBitmap bmp1, WriteableBitmap bmp2)
        {
            if (!bmp1.ToByteArray().Equals(bmp2.ToByteArray()))
            {
                return false;
            }
            for (int x = 0; x < bmp1.PixelWidth; ++x)
            {
                for (int y = 0; y < bmp1.PixelHeight; ++y)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
