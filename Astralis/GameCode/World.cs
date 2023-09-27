using Astralis.Configuration;
using Astralis.Configuration.Models;
using SadRogue.Primitives;
using System;
using Venomaus.FlowVitae.Chunking.Generators;
using Venomaus.FlowVitae.Grids;

namespace Astralis.GameCode
{
    internal class World : GridBase<byte, Tile, WorldChunk>
    {
        public Tile this[Point pos] => GetCell(pos.X, pos.Y);
        public Tile this[int x, int y] => this[(x, y)];
        public Tile this[int index] => this[Point.FromIndex(index, Width)];

        private readonly int _seed;

        /// <summary>
        /// Contains all data representing how to generate the world
        /// </summary>
        private readonly WorldGeneration _worldGenData = GameConfiguration.Load<WorldGeneration>();

        public World(int width, int height, int seed, IProceduralGen<byte, Tile, WorldChunk> generator) 
            : base(width, height, chunkWidth: 200, chunkHeight: 200, generator)
        {
            _seed = seed;
        }

        protected override Tile Convert(int x, int y, byte cellType)
        {
            var tile = base.Convert(x, y, cellType);

            // Retrieve chunk data + index of the coordinate on the elevation/moisture lookup array
            var chunkCoordinate = GetChunkCoordinate(x, y);
            var chunkData = GetChunkData(chunkCoordinate.x, chunkCoordinate.y);
            var screenCoordinate = WorldToScreenCoordinate(x, y);
            var index = screenCoordinate.y * Width + screenCoordinate.x;

            tile.Background = CalculateBackground(cellType, chunkData.Elevation[index], chunkData.Moisture[index]);
            return tile;
        }

        private Color CalculateBackground(byte tileId, float elevation, float moisture)
        {
            switch (tileId)
            {
                case (byte)TileType.Border:
                    return Color.AnsiMagentaBright;

                default:
                    // Get from json configuration
                    var tileType = (TileType)tileId;
                    if (!_worldGenData.Get.Biomes.TryGetValue(tileType, out var biome))
                    {
                        Console.WriteLine("Missing biome configuration: " + Enum.GetName(tileType));
                        return Color.Black;
                    }

                    return biome.BackgroundColor;
                    /*
                    // Check how we can handle neighbors with the chunks properly
                    // Maybe we only get the outer border of each neighbor chunk to compare for the current chunks border tiles?
                    // Check if this tile has neighboring tiles of the same type
                    bool hasLeftNeighbor = // TODO;
                    bool hasRightNeighbor = // TODO;
                    bool hasTopNeighbor = // TODO;
                    bool hasBottomNeighbor = // TODO;

                    // Define neighboring biome colors
                    Color leftNeighborColor = hasLeftNeighbor ? CalculateBackground(tileId, elevation, moisture) : biome.BackgroundColor;
                    Color rightNeighborColor = hasRightNeighbor ? CalculateBackground(tileId, elevation, moisture) : biome.BackgroundColor;
                    Color topNeighborColor = hasTopNeighbor ? CalculateBackground(tileId, elevation, moisture) : biome.BackgroundColor;
                    Color bottomNeighborColor = hasBottomNeighbor ? CalculateBackground(tileId, elevation, moisture) : biome.BackgroundColor;

                    // Blend the colors of the current tile and its neighbors
                    Color blendedColor = GetBiomeColor(biome.BackgroundColor, leftNeighborColor, elevation, moisture);
                    blendedColor = GetBiomeColor(blendedColor, rightNeighborColor, elevation, moisture);
                    blendedColor = GetBiomeColor(blendedColor, topNeighborColor, elevation, moisture);
                    blendedColor = GetBiomeColor(blendedColor, bottomNeighborColor, elevation, moisture);

                    return blendedColor;
                    */
            }
        }

        private static Color BlendColors(Color color1, Color color2, float blendFactor)
        {
            int r = (int)(color1.R * (1 - blendFactor) + color2.R * blendFactor);
            int g = (int)(color1.G * (1 - blendFactor) + color2.G * blendFactor);
            int b = (int)(color1.B * (1 - blendFactor) + color2.B * blendFactor);
            return new Color(r, g, b);
        }

        private static Color GetBiomeColor(Color biomeA, Color biomeB, float elevation, float moisture)
        {
            // Determine the transition zone
            float TransitionStart = 0.3f/* Define the start of the transition zone */;
            float TransitionEnd = 0.5f/* Define the end of the transition zone */;

            // Calculate the blend factor based on moisture (you can use elevation too)
            float blendFactor = (moisture - TransitionStart) / (TransitionEnd - TransitionStart);
            blendFactor = Math.Max(0, Math.Min(1, blendFactor)); // Ensure it stays in [0, 1] range

            // Blend the colors
            return BlendColors(biomeA, biomeB, blendFactor);
        }
    }
}
