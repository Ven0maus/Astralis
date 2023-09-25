using Astralis.Extended;
using GoRogue.FOV;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using System;

namespace Astralis.GameCode
{
    internal class Tile
    {
        public bool BlocksView;
        public bool IsExplored;
        public bool IsVisible;
        public Color Foreground;
        public Color Background;
    }

    internal class Map
    {
        public Tile this[Point pos] => _tiles[pos];
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, _width)];

        private readonly int _width, _height;
        private readonly ArrayView<Tile> _tiles;
        private readonly IFOV _fov;
        private readonly int _mapSeed;
        private readonly NoiseHelper _noise;

        public Map(int width, int height, int seed = 1337)
        {
            _width = width;
            _height = height;
            _tiles = new ArrayView<Tile>(width, height);
            _fov = new RecursiveShadowcastingFOV(new LambdaTranslationGridView<Tile, bool>(_tiles, x => x.BlocksView));
            _mapSeed = seed;
            _noise = new(_width, _height, _mapSeed);

            GenerateMap(false);
        }

        private void GenerateMap(bool applyIslandGradient)
        {
            var noiseMap = GetNoiseMap();
            var islandGradient = applyIslandGradient ? _noise.GenerateIslandGradientMap() : Array.Empty<float>();

            for (int x=0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (applyIslandGradient)
                        noiseMap[y * _width + x] -= islandGradient[y * _width + x];

                    _tiles[x, y] = GenerateTile(noiseMap[y * _width + x], applyIslandGradient);
                }
            }
        }

        private float[] GetNoiseMap()
        {
            var layer1 = _noise.GenerateNoiseMap(7, -0.79f, -0.75f, 1.64f);
            var layer2 = _noise.GenerateNoiseMap(10, -0.71f, 0.69f, 0.39f);
            var layer3 = _noise.GenerateNoiseMap(2, 0.33f, -1.18f, 0.93f);
            var layer4 = _noise.GenerateNoiseMap(4, 1.75f, -0.79f, 6.99f);

            var layer1Weight = 1f;
            var layer2Weight = 1f;
            var layer3Weight = 1f;
            var layer4Weight = 1f;

            Func<int, float> combiner = (index) => (layer1[index] * layer1Weight + layer2[index] * layer2Weight + layer3[index] * layer3Weight + layer4[index] * layer4Weight) / (layer1Weight + layer2Weight + layer3Weight + layer4Weight);

            // Initialize the combined map
            float[] combinedMap = new float[_width * _height];

            // Combine the noise maps into the final map
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = y * _width + x;

                    // Combine the noise maps using weighted averaging
                    combinedMap[index] = combiner(index);

                    // Ensure the combined value is within the [0, 1] range
                    combinedMap[index] = Mathf.Clamp01(combinedMap[index]);
                }
            }
            return combinedMap;
        }

        private Tile GenerateTile(float noise, bool islandApplied)
        {
            Color foreground = CalculateForeground(noise, islandApplied);
            Color background = CalculateBackground(noise, islandApplied);

            return new Tile { Foreground = foreground, Background = background };
        }

        private Color CalculateForeground(float noise, bool islandApplied)
        {
            // TODO
            return Color.White;
        }

        private Color CalculateBackground(float noise, bool islandApplied)
        {
            // TODO
            return Color.Lerp(Color.Black, Color.White, noise);
        }

        // TODO: Move this out of the map class into AI class
        public void UpdateFov(int x, int y, int radius = 5)
        {
            _fov.Calculate((x, y), radius, Distance.Euclidean);

            _fov.NewlySeen.ForEach((pos) =>
            {
                _tiles[pos].IsExplored = true;
                _tiles[pos].IsVisible = true;
            });

            _fov.NewlyUnseen.ForEach((pos) =>
            {
                _tiles[pos].IsVisible = false;
            });
        }
    }
}
