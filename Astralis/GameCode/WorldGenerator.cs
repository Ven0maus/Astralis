using Astralis.Extended;
using System;
using Venomaus.FlowVitae.Chunking;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Astralis.GameCode
{
    internal class WorldGenerator : IProceduralGen<byte, Tile>
    {
        public int Seed { get; }

        public WorldGenerator(int seed)
        { 
            Seed = seed; 
        }

        public (byte[] chunkCells, IChunkData chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            var random = new Random(seed);
            var noise = new NoiseHelper(seed);
            var chunk = new byte[width * height];

            float maxNoiseValue = float.MinValue;
            float minNoiseValue = float.MaxValue;
            var noiseMap = new float[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int chunkX = chunkCoordinate.x + x;
                    int chunkY = chunkCoordinate.y + y;
                    var noiseValue = noise.GetNoiseFromCombinedMaps(chunkX, chunkY);
                    noiseMap[y * width + x] = noiseValue;

                    maxNoiseValue = Math.Max(maxNoiseValue, noiseValue);
                    minNoiseValue = Math.Min(minNoiseValue, noiseValue);
                }
            }

            // Normalize the noise values to the range [0, 1]
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noiseMap[y * width + x] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[y * width + x]);
                }
            }

            SetTilemapByNoise(random, chunk, width, height, noiseMap);
            return (chunk, null);
        }

        private static void SetTilemapByNoise(Random random, byte[] chunk, int width, int height, float[] noise)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((x == 0 || y == 0 || x == width - 1 || y == height - 1) && Constants.DebugMode)
                    {
                        chunk[y * width + x] = (byte)TileType.Border;
                        continue;
                    }

                    chunk[y * width + x] = GetTileId(noise[y * width + x]);
                }
            }
        }

        private static byte GetTileId(float noise)
        {
            if (noise < 0.2f)
                return (byte)TileType.Water;
            if (noise < 0.3f)
                return (byte)TileType.Beach;
            if (noise < 0.45f)
                return (byte)TileType.Grasslands;
            if (noise < 0.65f)
                return (byte)TileType.Forest;
            if (noise < 0.80f)
                return (byte)TileType.DeepForest;
            if (noise < 0.90f)
                return (byte)TileType.Mountain;
            return (byte)TileType.Snow;
        }
    }
}
