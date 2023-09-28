using Astralis.Extended;
using System;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Astralis.GameCode
{
    internal class WorldGenerator : IProceduralGen<byte, Tile, WorldChunk>
    {
        public int Seed { get; }
        private readonly NoiseHelper _noise;

        public WorldGenerator(int seed)
        {
            Seed = seed;
            _noise = new NoiseHelper(seed);
        }

        public (byte[] chunkCells, WorldChunk chunkData) Generate(int seed, int width, int height, (int x, int y) chunkCoordinate)
        {
            // Create a unique random generator for this chunk
            var random = new Random(seed);

            // Create elevation and moisture lookup arrays
            var elevation = new float[width * height];
            var moisture = new float[width * height];

            SetNoisemap(width, height, chunkCoordinate,
                new NoiseData(elevation, _noise.GetElevation),
                new NoiseData(moisture, _noise.GetMoisture));

            // Set chunk based on provided lookup arrays
            var chunk = new byte[width * height];
            SetChunkValues(random, chunk, width, height, elevation, moisture);

            return (chunk, new WorldChunk(elevation, moisture));
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
                    if ((x == 0 || y == 0 || x == width - 1 || y == height - 1) && Constants.DebugMode)
                    {
                        chunk[index] = (byte)TileType.Border;
                        continue;
                    }

                    chunk[index] = (byte)GetTileType(elevation[index], moisture[index]);
                }
            }
        }

        private static TileType GetTileType(float elevation, float moisture)
        {
            if (elevation < 0.1) return TileType.Ocean;
            if (elevation < 0.12) return TileType.Beach;

            if (elevation > 0.8)
            {
                if (moisture < 0.1) return TileType.Scorched;
                if (moisture < 0.2) return TileType.Bare;
                if (moisture < 0.5) return TileType.Tundra;
                return TileType.Snow;
            }

            if (elevation > 0.6)
            {
                if (moisture < 0.33) return TileType.TemperateForest;
                if (moisture < 0.66) return TileType.Shrubland;
                return TileType.Taiga;
            }

            if (elevation > 0.3)
            {
                if (moisture < 0.16) return TileType.TemperateDesert;
                if (moisture < 0.50) return TileType.Grassland;
                if (moisture < 0.83) return TileType.TemperateForest;
                return TileType.TemperateRainForest;
            }

            if (moisture < 0.16) return TileType.SubtropicalDesert;
            if (moisture < 0.33) return TileType.Grassland;
            if (moisture < 0.66) return TileType.TropicalForest;
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
