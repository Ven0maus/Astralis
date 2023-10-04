using Astralis.Configuration.Models;
using Astralis.Extended;
using SadConsole;
using SadRogue.Primitives;
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

    internal class ObjectTile : ColoredGlyph, IEquatable<ObjectTile>
    {
        public byte CellType { get; set; }
        public bool BlocksView { get; set; }

        public ObjectTile(BiomeGeneration.BiomeObject obj, Random random)
        {
            var worldObject = obj.WorldObject;
            CellType = worldObject.Id;
            Glyph = worldObject.Glyphs.Random(random);
            Background = Color.Transparent;
            Foreground = obj.CopyBiomeColor != null && obj.CopyBiomeColor.Value ? obj.Biome.Color :
                worldObject.Colors.Random(random);
        }

        public bool Equals(ObjectTile other)
        {
            return other != null && CellType == other.CellType && BlocksView == other.BlocksView;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectTile);
        }

        public override int GetHashCode()
        {
            return CellType.GetHashCode() ^ BlocksView.GetHashCode();
        }
    }
}
