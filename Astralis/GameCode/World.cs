using SadRogue.Primitives;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
{
    internal class World : GridBase<byte, Tile>
    {
        public Tile this[Point pos] => GetCell(pos.X, pos.Y);
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, Width)];

        private readonly int _seed;

        public World(int width, int height, int seed, IProceduralGen<byte, Tile> generator) 
            : base(width, height, chunkWidth: 25, chunkHeight: 25, generator)
        {
            _seed = seed;
        }

        protected override Tile Convert(int x, int y, byte cellType)
        {
            var tile = base.Convert(x, y, cellType);
            tile.Background = CalculateBackground(cellType);
            return tile;
        }

        private static Color CalculateBackground(byte tileId)
        {
            if (tileId == (byte)TileType.Snow)
                return Color.Snow;
            if (tileId == (byte)TileType.Mountain)
                return Color.Gray;
            if (tileId == (byte)TileType.DeepForest)
                return Color.DarkGreen;
            if (tileId == (byte)TileType.Forest)
                return Color.Green;
            if (tileId == (byte)TileType.Grasslands)
                return Color.ForestGreen;
            if (tileId == (byte)TileType.Beach)
                return Color.AnsiYellowBright;
            if (tileId == (byte)TileType.Water)
                return Color.MidnightBlue;
            if (tileId == (byte)TileType.Border)
                return Color.AnsiMagentaBright;
            return Color.Black;
        }
    }
}
