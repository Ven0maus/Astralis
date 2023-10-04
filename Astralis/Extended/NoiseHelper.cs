using Newtonsoft.Json.Linq;
using System;

namespace Astralis.Extended
{
    public class NoiseHelper
    {
        private readonly FastNoiseLite _noise;

        public NoiseHelper(int seed)
        {
            _noise = new FastNoiseLite(seed);
        }

        public float GetElevation(float x, float y)
        {
            return GetNoise(x, y, 7, 3.25f, -0.75f, 1.64f);

        }

        public float GetMoisture(float x, float y)
        {
            return GetNoise(x, y, 10, 1.70f, 0.69f, 0.39f);
        }

        public float GetRivers(float x, float y)
        {
            var noise = GetNoise(x, y, 1, 1.5f, 0.58f, 1.5f, false);
            return Mathf.Remap(-1 * Math.Abs(noise), -1f, 1f, 0f, 1f);
        }

        public void SetNoiseType(FastNoiseLite.NoiseType noiseType)
        {
            _noise.SetNoiseType(noiseType);
        }

        public float GetNoise(float x, float y, int octaves, float scale, float persistence, float lacunarity, bool reMap = true)
        {
            float amplitude = 1.0f;
            float frequency = 1.0f;
            float noiseValue = 0.0f;

            for (int octave = 0; octave < octaves; octave++)
            {
                float sampleX = x / scale * frequency;
                float sampleY = y / scale * frequency;

                noiseValue += _noise.GetNoise(sampleX, sampleY) * amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return reMap ? Mathf.Remap(noiseValue, -1f, 1f, 0f, 1f) : noiseValue;
        }
    }
}
