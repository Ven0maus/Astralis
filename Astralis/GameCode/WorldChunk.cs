using Astralis.Configuration.Models;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Venomaus.FlowVitae.Chunking;

namespace Astralis.GameCode
{
    internal class WorldChunk : IChunkData
    {
        public int Seed { get; set; }
        public (int x, int y) ChunkCoordinate { get; set; }
        public Color[] BiomeColors { get; private set; }

        private readonly WorldObject[] _objects;
        public readonly int Width, Height;

        public WorldChunk(byte[] biomes, WorldObject[] objects, int chunkWidth, int chunkHeight)
        {
            _objects = objects;
            Width = chunkWidth;
            Height = chunkHeight;
            BiomeColors = new Color[Width * Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    BiomeColors[y * Width + x] = CalculateBiomeColor((ChunkCoordinate.x + x, ChunkCoordinate.y + y), biomes[y * Width + x], biomes);
                }
            }
        }

        public WorldObject GetObject(int x, int y)
        {
            return _objects[(y - ChunkCoordinate.y) * Width + (x - ChunkCoordinate.x)];
        }

        public Color GetBiomeColor(int x, int y)
        {
            return BiomeColors[(y - ChunkCoordinate.y) * Width + (x - ChunkCoordinate.x)];
        }

        private static IEnumerable<(int x, int y)> GetNeighborBorderPositions(int chunkWidth, int chunkHeight)
        {
            int newWidth = chunkWidth + 2;  // New width
            int newHeight = chunkHeight + 2; // New height

            List<(int x, int y)> borderCoordinates = new();

            // Iterate through the top and bottom rows
            for (int x = 0; x < newWidth; x++)
            {
                borderCoordinates.Add((x - 1, -1));
                borderCoordinates.Add((x - 1, newHeight - 2));
            }

            // Iterate through the left and right columns, excluding the corners
            for (int y = 1; y < newHeight - 1; y++)
            {
                borderCoordinates.Add((-1, y - 1));
                borderCoordinates.Add((newWidth - 2, y - 1));
            }

            return borderCoordinates;
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
                    var neighbors = new[]
                    {
                        leftNeighbor,
                        rightNeighbor,
                        topNeighbor,
                        bottomNeighbor
                    }
                        .Where(a => a != null)
                        .Select(a => new { Color = GetTileBaseColor(a.Value, false), Obj = a })
                        .DistinctBy(a => a.Color);

                    Color blendedColor = baseColor;
                    bool baseColorSet = false;
                    foreach (var neighbor in neighbors)
                    {
                        if (!baseColorSet)
                            baseColorSet = true;
                        blendedColor = GetBiomeColor(!baseColorSet ? baseColor : blendedColor, neighbor.Color);
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
                    coordinate = (coordinate.x, y: coordinate.y + 1);
                    break;
                case Dir.Down:
                    coordinate = (coordinate.x, y: coordinate.y - 1);
                    break;
                case Dir.Left:
                    coordinate = (coordinate.x - 1, y: coordinate.y);
                    break;
                case Dir.Right:
                    coordinate = (coordinate.x + 1, y: coordinate.y);
                    break;
            }

            int index = coordinate.y * Width + coordinate.x;
            return InBoundsNeighbor(coordinate.x, coordinate.y) ? biomes[index] : null;
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

        private static Color BlendColors(Color color1, Color color2, float blendFactor)
        {
            int r = (int)(color1.R * (1 - blendFactor) + color2.R * blendFactor);
            int g = (int)(color1.G * (1 - blendFactor) + color2.G * blendFactor);
            int b = (int)(color1.B * (1 - blendFactor) + color2.B * blendFactor);
            return new Color(r, g, b);
        }

        private static Color GetBiomeColor(Color biomeA, Color biomeB)
        {
            // Adjust the blend factor as needed
            float blendFactor = 0.12f;

            // Blend the colors
            return BlendColors(biomeA, biomeB, blendFactor);
        }
    }
}
