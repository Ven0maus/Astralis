using System;

namespace Astralis.Extended
{
    public class NoiseHelper
    {
        private readonly int _width, _height;
        private readonly FastNoiseLite _noise;

        public NoiseHelper(int width, int height, int seed)
        {
            _width = width;
            _height = height;
            _noise = new FastNoiseLite(seed);
        }

        /// <summary>
        /// Generates a noise map of values between 0...1 based on the provided parameters.
        /// </summary>
        /// <param name="scale">Controls the overall scale of the noise</param>
        /// <param name="octaves">Number of octaves for the noise</param>
        /// <param name="persistence">Controls how quickly the amplitudes decrease</param>
        /// <param name="lacunarity">Controls how quickly the frequencies increase</param>
        /// <returns>float[] of values between 0...1</returns>
        public float[] GenerateNoiseMap(int octaves, float scale, float persistence, float lacunarity)
        {
            // Need atleast one octave
            if (octaves <= 0)
                octaves = 1;

            float[] noiseMap = new float[_width * _height];
            float maxNoiseValue = float.MinValue;
            float minNoiseValue = float.MaxValue;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
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

                    // Track the maximum and minimum noise values
                    maxNoiseValue = Math.Max(maxNoiseValue, noiseValue);
                    minNoiseValue = Math.Min(minNoiseValue, noiseValue);

                    noiseMap[y * _width + x] = noiseValue;
                }
            }

            // Normalize the noise values to the range [0, 1]
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    noiseMap[y * _width + x] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[y * _width + x]);
                }
            }

            return noiseMap;
        }

        public float[] GenerateIslandGradientMap()
        {
            float[] map = new float[_width * _height];
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // Value between 0 and 1 where * 2 - 1 makes it between -1 and 0
                    float i = x / (float)_width * 2 - 1;
                    float j = y / (float)_height * 2 - 1;

                    // Find closest x or y to the edge of the map
                    float value = Math.Max(Math.Abs(i), Math.Abs(j));

                    // Apply a curve graph to have more values around 0 on the edge, and more values >= 3 in the middle
                    float a = 3;
                    float b = 2.2f;
                    float islandGradientValue = (float)Math.Pow(value, a) / ((float)Math.Pow(value, a) + (float)Math.Pow(b - b * value, a));

                    // Apply gradient in the map
                    map[y * _width + x] = islandGradientValue;
                }
            }
            return map;
        }
    }
}
