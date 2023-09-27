﻿using System;

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
            return GetNoise(x, y, 7, -0.79f, -0.75f, 1.64f);

        }

        public float GetMoisture(float x, float y)
        {
            return GetNoise(x, y, 10, -0.71f, 0.69f, 0.39f);
        }

        private float GetNoise(float x, float y, int octaves, float scale, float persistence, float lacunarity)
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
