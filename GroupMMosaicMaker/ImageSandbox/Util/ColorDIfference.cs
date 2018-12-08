using System;
using Windows.UI;

namespace ImageSandbox.Util
{
    internal class ColorDifference
    {
        #region Methods

        /// <summary>
        ///     Finds the difference of the two given colors
        /// </summary>
        /// <param name="color1">The first color.</param>
        /// <param name="color2">The second color.</param>
        /// <returns>The value in relation to the difference between the two colors </returns>
        public static int GetColorDifference(Color color1, Color color2)
        {
            return Math.Abs(color2.R - color1.R) + Math.Abs(color2.G - color1.G) + Math.Abs(color2.B - color1.B);
        }

        #endregion
    }
}