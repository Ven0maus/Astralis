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

            GenerateMap();
        }

        private void GenerateMap()
        {
            var heightMap = _noise.GenerateNoiseMap(12, 0.24f, 0.45f, 2.2f);
            var islandGradient = _noise.GenerateIslandGradientMap();

            // Add moisture, heat maps?
            
            for (int x=0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    heightMap[y * _width + x] -= islandGradient[y * _width + x];
                    _tiles[x, y] = new Tile { Foreground = Color.White, Background = Color.Lerp(Color.Black, Color.White, heightMap[y * _width + x]) };
                }
            }
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
