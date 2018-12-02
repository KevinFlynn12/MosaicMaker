using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageSandbox.Util
{
    class ColorDIfference
    {
        public static int GetColorDifference(Color color, Color baseColor)
        {
            return Math.Abs(baseColor.R - color.R) + Math.Abs(baseColor.G - color.G) + Math.Abs(baseColor.B - color.B);
        }

    }
}
