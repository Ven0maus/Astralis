using Astralis.Extended;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Noise.Visualizer
{
    internal class NoiseScreen : ScreenSurface
    {
        private int _seed;
        public int Seed
        {
            get
            {
                return _seed;
            }
            set
            {
                if (_seed != value)
                {
                    _seed = value;
                    _noise = new NoiseHelper(Width, Height, _seed);
                }
            }
        }
        public int Octaves { get; set; }
        public float Persistance { get; set; }
        public float Lacunarity { get; set; }
        public float Scale { get; set; }
        public bool ApplyIslandGradient { get; set; } = true;

        private NoiseHelper _noise;

        public NoiseScreen(int width, int height, int seed = 1337) : base(width, height) 
        {
            Octaves = 1;
            Persistance = 0.5f;
            Lacunarity = 2.0f;
            Scale = 0.35f;
            Seed = seed;
        }

        public void Draw()
        {
            var noiseMap = _noise.GenerateNoiseMap(Octaves, Scale, Persistance, Lacunarity);
            var islandGradient = ApplyIslandGradient ? _noise.GenerateIslandGradientMap() : Array.Empty<float>();

            for (int x=0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (ApplyIslandGradient)
                        noiseMap[y * Width + x] -= islandGradient[y * Width + x];

                    Surface[x, y].Background = Color.Lerp(Color.White, Color.Black, noiseMap[y * Width + x]);
                    Surface[x, y].IsDirty = true;
                }
            }
            IsDirty = true;
        }
    }
}
