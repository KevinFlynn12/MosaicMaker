using Windows.UI;

namespace ImageSandbox.Util
{
    public static class ImagePixel
    {
        #region Methods

        /// <summary>
        ///     Gets the pixel bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns>The color of the area that you want</returns>
        public static Color GetPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int) width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        /// <summary>
        ///     Sets the pixel bgra8.
        /// </summary>
        /// <param name="pixels">The pixels.</param>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="isBlackAndWhite">if set to <c>true</c> [is black and white].</param>
        public static void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height,
            bool isBlackAndWhite)
        {
            var offset = (x * (int) width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        #endregion
    }
}