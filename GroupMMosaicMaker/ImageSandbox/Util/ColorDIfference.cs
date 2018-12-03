using System;
using Windows.UI;

namespace ImageSandbox.Util
{
    internal class ColorDifference
    {
        #region Methods

        /// <summary>
        ///     Subtracts the base color1 from color2
        ///     to find the diference in values from red
        ///     blue and green
        /// </summary>
        /// <param name="color1">The color1.</param>
        /// <param name="color2">Color of the base.</param>
        /// <returns>The value in relation to the difference in color1 </returns>
        public static int GetColorDifference(Color color1, Color color2)
        {
            return Math.Abs(color2.R - color1.R) + Math.Abs(color2.G - color1.G) + Math.Abs(color2.B - color1.B);
        }

        #endregion
    }
}