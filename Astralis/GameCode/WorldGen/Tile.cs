using SadConsole;
using System;
using Venomaus.FlowVitae.Cells;

namespace Astralis.GameCode.WorldGen
{
    internal class Tile : ColoredGlyph, ICell<byte>, IEquatable<ICell<byte>>, IEquatable<(int x, int y)>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public byte CellType { get; set; }
        public bool BlocksView
        {
            get { return Object != null && Object.BlocksView; }
        }
        public bool Walkable
        {
            get
            {
                if (CellType == (int)BiomeType.Ocean || CellType == (int)BiomeType.River)
                    return false;
                return Object == null || Object.Walkable;
            }
        }
        public bool IsExplored { get; set; }
        public ObjectTile Object { get; set; }

        public Tile() : base() { }

        public Tile(int x, int y, byte cellType)
        {
            X = x;
            Y = y;
            CellType = cellType;
        }

        public bool Equals(ICell<byte> other)
        {
            return other != null && X == other.X && Y == other.Y;
        }

        public bool Equals((int x, int y) other)
        {
            return X == other.x && Y == other.y;
        }
    }
}
