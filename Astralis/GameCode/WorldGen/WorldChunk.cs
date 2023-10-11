using Astralis.Configuration.Models;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Venomaus.FlowVitae.Chunking;

namespace Astralis.GameCode.WorldGen
{
    internal class WorldChunk : IChunkData
    {
        public int Seed { get; set; }
        public (int x, int y) ChunkCoordinate { get; set; }
        public Color[] BiomeColors { get; private set; }

        private readonly int[] _objects;

        public readonly int Width, Height;

        private static readonly Dictionary<int, ObjectTile> _objectTileCache = new Dictionary<int, ObjectTile>();

        public WorldChunk(byte[] biomes, BiomeGeneration.BiomeObject[] objects, int chunkWidth, int chunkHeight, Random random)
        {
            Width = chunkWidth;
            Height = chunkHeight;
            BiomeColors = new Color[Width * Height];
            _objects = new int[Width * Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    BiomeColors[y * Width + x] = CalculateBiomeColor((ChunkCoordinate.x + x, ChunkCoordinate.y + y), biomes[y * Width + x], biomes);

                    var obj = objects[y * Width + x];
                    if (obj != null)
                    {
                        _objects[y * Width + x] = GetCachedObjectTile(obj, random);
                    }
                }
            }
        }

        public static ObjectTile GetObjectTileById(int id)
        {
            if (!_objectTileCache.TryGetValue(id, out var tile))
            {
                return null;
            }
            return tile;
        }

        private static int GetCachedObjectTile(BiomeGeneration.BiomeObject obj, Random random)
        {
            var key = new ObjectTileKey(obj, random);
            var hash = key.GetHashCode();
            if (!_objectTileCache.ContainsKey(hash))
            {
                var tile = new ObjectTile(key);
                _objectTileCache[hash] = tile;
            }
            return hash;
        }

        public int GetObject(int x, int y)
        {
            return _objects[(y - ChunkCoordinate.y) * Width + (x - ChunkCoordinate.x)];
        }

        public Color GetBiomeColor(int x, int y)
        {
            return BiomeColors[(y - ChunkCoordinate.y) * Width + (x - ChunkCoordinate.x)];
        }

        private static Color GetTileBaseColor(byte tileId, bool throwExceptionOnMissingConfiguration = true)
        {
            if (!World.BiomeData.Get.Biomes.TryGetValue((BiomeType)tileId, out var biome))
            {
                if (throwExceptionOnMissingConfiguration)
                    throw new Exception("Missing biome configuration: " + Enum.GetName((BiomeType)tileId));
                return Color.AnsiMagentaBright;
            }
            return biome.Color;
        }

        private Color CalculateBiomeColor((int x, int y) pos, byte cellType, byte[] biomes)
        {
            switch (cellType)
            {
                case (byte)BiomeType.Border:
                    return Color.AnsiMagentaBright;

                default:
                    // Get from json configuration
                    var baseColor = GetTileBaseColor(cellType);

                    var leftNeighbor = GetNeighborTileData(pos, Dir.Left, biomes);
                    var rightNeighbor = GetNeighborTileData(pos, Dir.Right, biomes);
                    var topNeighbor = GetNeighborTileData(pos, Dir.Up, biomes);
                    var bottomNeighbor = GetNeighborTileData(pos, Dir.Down, biomes);

                    // Define neighboring biome colors that are different from eachother
                    var neighborColors = new[]
                    {
                        leftNeighbor,
                        rightNeighbor,
                        topNeighbor,
                        bottomNeighbor
                    }
                        .Where(a => a != null)
                        .Select(a => GetTileBaseColor(a.Value, false))
                        .Distinct();

                    Color blendedColor = baseColor;
                    bool baseColorSet = false;
                    foreach (var color in neighborColors)
                    {
                        if (color == baseColor) continue;
                        if (!baseColorSet)
                            baseColorSet = true;
                        blendedColor = GetBiomeColor(!baseColorSet ? baseColor : blendedColor, color);
                    }

                    return blendedColor;
            }
        }

        private byte? GetNeighborTileData((int x, int y) pos, Dir direction, byte[] biomes)
        {
            (int x, int y) coordinate = (x: pos.x - ChunkCoordinate.x, y: pos.y - ChunkCoordinate.y);

            switch (direction)
            {
                case Dir.Up:
                    coordinate = (coordinate.x, coordinate.y + 1);
                    break;
                case Dir.Down:
                    coordinate = (coordinate.x, coordinate.y - 1);
                    break;
                case Dir.Left:
                    coordinate = (coordinate.x - 1, coordinate.y);
                    break;
                case Dir.Right:
                    coordinate = (coordinate.x + 1, coordinate.y);
                    break;
            }

            int index = coordinate.y * Width + coordinate.x;
            var inBounds = InBoundsNeighbor(coordinate.x, coordinate.y);
            if (inBounds)
                return biomes[index];
            return null;
        }

        private bool InBoundsNeighbor(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        enum Dir
        {
            Up,
            Down,
            Left,
            Right
        }

        private static Color BlendColors(Color color1, Color color2, float blendFentity)
        {
            int r = (int)(color1.R * (1 - blendFentity) + color2.R * blendFentity);
            int g = (int)(color1.G * (1 - blendFentity) + color2.G * blendFentity);
            int b = (int)(color1.B * (1 - blendFentity) + color2.B * blendFentity);
            return new Color(r, g, b);
        }

        private static Color GetBiomeColor(Color biomeA, Color biomeB)
        {
            // Adjust the blend fentity as needed
            float blendFentity = 0.12f;

            // Blend the colors
            return BlendColors(biomeA, biomeB, blendFentity);
        }
    }
}
