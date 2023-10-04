using Astralis.Configuration.Models;
using Astralis.Extended;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Astralis.GameCode.WorldGen
{
    internal class ObjectTile : ColoredGlyph, IEquatable<ObjectTile>
    {
        public byte CellType { get; set; }
        public bool BlocksView { get; set; }

        public ObjectTile(ObjectTileKey key)
        {
            CellType = key.CellType;
            Glyph = key.Glyph;
            BlocksView = key.BlocksView;
            Background = key.Background;
            Foreground = key.Foreground;
        }

        public bool Equals(ObjectTile other)
        {
            return other != null &&
                CellType == other.CellType &&
                BlocksView == other.BlocksView;
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

    /// <summary>
    /// Similar to ObjectTile but used as a key to hold a cached version to the similar ObjectTile.
    /// </summary>
    internal class ObjectTileKey : ColoredGlyph, IEquatable<ObjectTileKey>
    {
        public byte CellType { get; set; }
        public bool BlocksView { get; set; }

        public ObjectTileKey(BiomeGeneration.BiomeObject obj, Random random)
        {
            var worldObject = obj.WorldObject;
            CellType = worldObject.Id;
            Glyph = worldObject.Glyphs.Random(random);
            BlocksView = worldObject.BlocksView;
            Background = Color.Transparent;
            Foreground = obj.CopyBiomeColor != null && obj.CopyBiomeColor.Value ? obj.Biome.Color :
                worldObject.Colors.Random(random);
        }

        public bool Equals(ObjectTileKey other)
        {
            return other != null &&
                CellType == other.CellType &&
                BlocksView == other.BlocksView &&
                Glyph == other.Glyph &&
                Background == other.Background &&
                Foreground == other.Foreground;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectTileKey);
        }

        public override int GetHashCode()
        {
            return CellType.GetHashCode() ^ BlocksView.GetHashCode();
        }
    }
}
