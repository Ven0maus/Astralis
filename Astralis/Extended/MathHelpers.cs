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

        public static float Remap(float value, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            float t = (value - fromSource) / (toSource - fromSource);
            if (t > 1f)
                return toTarget;
            if (t < 0f)
                return fromTarget;
            return fromTarget + (toTarget - fromTarget) * t;
        }

        public static float Clamp01(float value)
        {
            return (value < 0.0f) ? 0.0f : (value > 1.0f) ? 1.0f : value;
        }

        public static int PercentOf(int value, int percent)
        {
            return (int)((float)value / 100 * percent);
        }

        public static float Smoothstep(float t)
        {
            // Apply the smoothstep formula
            return t * t * (3 - 2 * t);
        }

        public static float Smoothstep(float min, float max, float t)
        {
            // Ensure t is clamped between 0 and 1
            t = Mathf.Clamp01((t - min) / (max - min));

            // Apply the smoothstep formula
            return t * t * (3 - 2 * t);
        }
    }
}
