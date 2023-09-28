using Astralis.Extended;
using System;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Astralis.GameCode
{
    internal class WorldGenerator : IProceduralGen<byte, Tile, WorldChunk>
    {
        public int Seed { get; }
        public readonly NoiseHelper NoiseHelper;

        public WorldGenerator(int seed, NoiseHelper noiseHelper)
        {
            Seed = seed;
            NoiseHelper = noiseHelper;
        }

        public (byte[] chunkCells, WorldChunk chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Create a unique random generator for this chunk
            var random = new Random(seed);

            // Create elevation and moisture lookup arrays
            var elevation = new float[width * height];
            var moisture = new float[width * height];

            SetNoisemap(width, height, chunkCoordinate,
                new NoiseData(elevation, NoiseHelper.GetElevation),
                new NoiseData(moisture, NoiseHelper.GetMoisture));

            // Set chunk based on provided lookup arrays
            var chunk = new byte[width * height];
            SetChunkValues(random, chunk, width, height, elevation, moisture);

            return (chunk, new WorldChunk(chunk, width, height));
        }

        private static void SetNoisemap(int width, int height, (int x, int y) chunkCoordinate, params NoiseData[] noiseData)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float chunkX = chunkCoordinate.x + x;
                    float chunkY = chunkCoordinate.y + y;

                    foreach (var data in noiseData)
                    {
                        var noiseValue = data.NoiseFunc(chunkX, chunkY);

                        data.Noisemap[y * width + x] = noiseValue;
                        data.MaxValue = Math.Max(data.MaxValue, noiseValue);
                        data.MinValue = Math.Min(data.MinValue, noiseValue);
                    }
                }
            }
        }

        private static void SetChunkValues(Random random, byte[] chunk, int width, int height, float[] elevation, float[] moisture)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if ((x == 0 || y == 0 || x == width - 1 || y == height - 1) && Constants.DebugMode && Constants.WorldGeneration.DrawBordersOnDebugMode)
                    {
                        chunk[index] = (byte)TileType.Border;
                        continue;
                    }

                    chunk[index] = (byte)GetTileType(elevation[index], moisture[index]);
                }
            }
        }

        public static TileType GetTileType(float elevation, float moisture)
        {
            if (elevation < 0.05 || (moisture > 0.95 && elevation < 0.2)) return TileType.Ocean;
            if (elevation < 0.2) return TileType.Beach;

            if (elevation > 0.9)
            {
                if (moisture < 0.1) return TileType.Scorched;
                if (moisture < 0.25) return TileType.Bare;
                if (moisture < 0.6) return TileType.Tundra;
                return TileType.Snow;
            }

            if (elevation > 0.7)
            {
                if (moisture < 0.4) return TileType.TemperateForest;
                if (moisture < 0.75) return TileType.Shrubland;
                return TileType.Taiga;
            }

            if (elevation > 0.4)
            {
                if (moisture < 0.2) return TileType.TemperateDesert;
                if (moisture < 0.55) return TileType.Grassland;
                if (moisture < 0.8) return TileType.TemperateForest;
                return TileType.TemperateRainForest;
            }

            if (moisture < 0.2) return TileType.SubtropicalDesert;
            if (moisture < 0.4) return TileType.Grassland;
            if (moisture < 0.7) return TileType.TropicalForest;
            return TileType.TropicalRainForest;
        }

        class NoiseData
        {
            public readonly float[] Noisemap;
            public readonly Func<float, float, float> NoiseFunc;

            public float MinValue;
            public float MaxValue;

            public NoiseData(float[] noiseMap, Func<float, float, float> noiseFunc)
            {
                Noisemap = noiseMap;
                NoiseFunc = noiseFunc;
                MinValue = float.MaxValue;
                MaxValue = float.MinValue;
            }
        }
    }
}
