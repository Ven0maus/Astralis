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
            var heightMap = _noise.GenerateNoiseMap(10, 1.0f, 0.5f, 2.0f);
            var moistureMap = _noise.GenerateNoiseMap(8, 0.85f, 0.2f, 1.5f);
            var heatMap = _noise.GenerateNoiseMap(6, 0.10f, 0.3f, 2.0f);
            var islandGradient = applyIslandGradient ? _noise.GenerateIslandGradientMap() : Array.Empty<float>();

            for (int x=0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (applyIslandGradient)
                        heightMap[y * _width + x] -= islandGradient[y * _width + x];

                    float height = heightMap[y * _width + x];
                    float moisture = moistureMap[y * _width + x];
                    float heat = heatMap[y * _width + x];

                    _tiles[x, y] = GenerateTile(height, moisture, heat, applyIslandGradient);
                }
            }
        }

        private Tile GenerateTile(float height, float moisture, float heat, bool islandApplied)
        {
            Color foreground = CalculateForeground(height, moisture, heat, islandApplied);
            Color background = CalculateBackground(height, moisture, heat, islandApplied);

            return new Tile { Foreground = foreground, Background = background };
        }

        private Color CalculateForeground(float height, float moisture, float heat, bool islandApplied)
        {
            // TODO
            return Color.White;
        }

        private Color CalculateBackground(float height, float moisture, float heat, bool islandApplied)
        {
            // TODO
            return Color.Lerp(Color.Black, Color.White, height);
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
