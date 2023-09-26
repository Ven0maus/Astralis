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

        public float GetNoiseFromCombinedMaps(float x, float y)
        {
            var elevationWeight = 0.7f;
            var moistureWeight = 0.4f;
            var heatWeight = 0.2f;

            var elevationNoise = GetNoise(x, y, 7, -0.79f, -0.75f, 1.64f);
            var moistureNoise = GetNoise(x, y, 10, -0.71f, 0.69f, 0.39f);
            var heatNoise = GetNoise(x, y, 2, 0.33f, -1.18f, 0.93f);

            var noiseValue =
                ((elevationNoise * elevationWeight) +
                (moistureNoise * moistureWeight) +
                (heatNoise * heatWeight)) /
                (elevationWeight + moistureWeight + heatWeight);

            return noiseValue;
        }

        public float GetNoise(float x, float y, int octaves, float scale, float persistence, float lacunarity)
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

            return noiseValue;
        }       
    }
}
