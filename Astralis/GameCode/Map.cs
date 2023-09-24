using Astralis.Extended;
using GoRogue.FOV;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace Astralis.GameCode
{
    internal class Tile
    {
        public bool BlocksView;
        public bool IsExplored;
        public bool IsVisible;
    }

    internal class Map
    {
        public Tile this[Point pos] => _tiles[pos];
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, _width)];

        private readonly int _width, _height;
        private readonly ArrayView<Tile> _tiles;
        private readonly IFOV _fov;

        public Map(int width, int height)
        {
            _width = width;
            _height = height;
            _tiles = new ArrayView<Tile>(width, height);
            _fov = new RecursiveShadowcastingFOV(new LambdaTranslationGridView<Tile, bool>(_tiles, x => x.BlocksView));
        }

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
