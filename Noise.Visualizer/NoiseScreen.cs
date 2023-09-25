using Astralis.Extended;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    Noise = new NoiseHelper(Width, Height, _seed);
                }
            }
        }

        public bool ApplyIslandGradient { get; set; } = true;

        public NoiseHelper Noise { get; private set; }

        private readonly List<NoiseMapConfiguration> _configuration = new();
        public IReadOnlyList<NoiseMapConfiguration> Configuration { get { return _configuration; } }

        public NoiseMapConfiguration Current { get; private set; }

        public class NoiseMapConfiguration
        {
            public string Name { get; }
            public int Octaves { get; set; }
            public float Persistance { get; set; }
            public float Lacunarity { get; set; }
            public float Scale { get; set; }
            public float Weight { get; set; }

            private readonly Func<NoiseHelper> _noiseGetter;
            public float[] Noise { get { return _noiseGetter().GenerateNoiseMap(Octaves, Scale, Persistance, Lacunarity); } }

            public NoiseMapConfiguration(Func<NoiseHelper> noiseGetter, string name, int octaves, float scale, float persistance, float lacunarity, float weight)
            {
                _noiseGetter = noiseGetter;
                Name = name;
                Octaves = octaves;
                Persistance = persistance;
                Lacunarity = lacunarity;
                Scale = scale;
                Weight = weight;
            }

            public NoiseMapConfiguration Clone()
            {
                return new NoiseMapConfiguration(_noiseGetter, Name, Octaves, Scale, Persistance, Lacunarity, Weight);
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public NoiseScreen(int width, int height, int seed = 0) : base(width, height) 
        {
            _seed = seed;
            Noise = new NoiseHelper(width, height, seed);
            AddNoiseMap("layer1");
        }

        public NoiseMapConfiguration AddNoiseMap(string name)
        {
            Current = new NoiseMapConfiguration(() => Noise, name, 1, 0.35f, 0.5f, 2.0f, 1f);
            _configuration.Add(Current);
            return Current;
        }

        public void AddNoiseMap(NoiseMapConfiguration config)
        {
            _configuration.Add(config);
        }

        private float[] GetNoiseMap()
        {
            // Initialize the combined map
            var noiseMaps = Configuration.Select(a => new { a.Noise, a.Weight }).ToArray();
            float[] combinedMap = new float[Width * Height];

            // Combine the noise maps into the final map
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;

                    // Combine the noise maps using weighted averaging
                    float combinedValue = noiseMaps.Sum(a => a.Noise[index] * a.Weight);
                    combinedValue /= noiseMaps.Sum(a => a.Weight);
                    combinedMap[index] = combinedValue;

                    // Ensure the combined value is within the [0, 1] range
                    combinedMap[index] = Mathf.Clamp01(combinedMap[index]);
                }
            }
            return combinedMap;
        }

        public void Draw()
        {
            var noiseMap = GetNoiseMap();
            var islandGradient = ApplyIslandGradient ? Noise.GenerateIslandGradientMap() : Array.Empty<float>();

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

        internal void SetCurrent(NoiseMapConfiguration selected)
        {
            Current = selected;
        }

        internal NoiseMapConfiguration RemoveNoiseMap(NoiseMapConfiguration current)
        {
            _configuration.Remove(current);
            if (Current == current)
                Current = _configuration.LastOrDefault();
            return current;
        }

        internal void ClearNoiseMaps()
        {
            _configuration.Clear();
            Current = null;
        }
    }
}
