using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astralis.Extended
{
    public static class Mathf
    {
        public static float InverseLerp(float a, float b, float value)
        {
            if (a != b)
            {
                return Clamp01((value - a) / (b - a));
            }
            else
            {
                // Handle the case where a and b are equal to avoid division by zero
                return 0.0f;
            }
        }

        public static float Clamp01(float value)
        {
            return (value < 0.0f) ? 0.0f : (value > 1.0f) ? 1.0f : value;
        }

        public static int PercentOf(int value, int percent)
        {
            return (int)((float)value / 100 * percent);
        }
    }
}
