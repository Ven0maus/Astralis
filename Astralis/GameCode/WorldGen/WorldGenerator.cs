using Astralis.Configuration.Models;
using Astralis.Extended;
using System;
using System.Linq;
using Venomaus.FlowVitae.Chunking.Generators;

namespace Astralis.GameCode.WorldGen
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
            var biomes = new byte[width * height];
            var objects = new BiomeGeneration.BiomeObject[width * height];
            SetChunkValues(random, biomes, objects, width, height, elevation, moisture);

            return (biomes, new WorldChunk(biomes, objects, width, height, random));
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
                    }
                }
            }
        }

        private static void SetChunkValues(Random random, byte[] biomes, BiomeGeneration.BiomeObject[] objects, int width, int height, float[] elevation, float[] moisture)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if ((x == 0 || y == 0 || x == width - 1 || y == height - 1) && Constants.DebugMode && Constants.WorldGeneration.DrawBordersOnDebugMode)
                    {
                        biomes[index] = (byte)BiomeType.Border;
                        continue;
                    }

                    var biomeType = GetTileType(elevation[index], moisture[index]);
                    var objectType = GetRandomBiomeObject(random, biomeType, true);
                    biomes[index] = (byte)biomeType;
                    objects[index] = objectType;
                }
            }

            CellularAutomaton.Apply(objects, biomes, width, height, 3);
        }

        private static BiomeGeneration.BiomeObject GetRandomBiomeObject(Random random, BiomeType biomeType, bool applyRarity)
        {
            if (World.BiomeData.Get.Biomes.TryGetValue(biomeType, out var biome) && biome.Objects != null)
            {
                var randomObject = biome.Objects
                    .Where(a => !applyRarity || random.Next(0, GetRandomRollByRarity(a.SpawnRarity)) < a.SpawnChance)
                    .RandomOrDefault(random);
                if (randomObject != null)
                {
                    return randomObject;
                }
            }
            return null;
        }

        private static int GetRandomRollByRarity(BiomeGeneration.Rarity rarity)
        {
            switch (rarity)
            {
                case BiomeGeneration.Rarity.Normal: return 100;
                case BiomeGeneration.Rarity.Uncommon: return 200;
                case BiomeGeneration.Rarity.Rare: return 300;
                case BiomeGeneration.Rarity.Epic: return 500;
                case BiomeGeneration.Rarity.Legendary: return 750;
                case BiomeGeneration.Rarity.Mythical: return 1000;
            }
            return 100;
        }

        public static BiomeType GetTileType(float elevation, float moisture)
        {
            if (elevation < 0.05 || moisture > 0.95 && elevation < 0.2) return BiomeType.Ocean;
            if (elevation < 0.2) return BiomeType.Beach;

            if (elevation > 0.9)
            {
                if (moisture < 0.1) return BiomeType.Scorched;
                if (moisture < 0.25) return BiomeType.Bare;
                if (moisture < 0.6) return BiomeType.Tundra;
                return BiomeType.Snow;
            }

            if (elevation > 0.7)
            {
                if (moisture < 0.4) return BiomeType.TemperateForest;
                if (moisture < 0.75) return BiomeType.Swamp;
                return BiomeType.Taiga;
            }

            if (elevation > 0.4)
            {
                if (moisture < 0.2) return BiomeType.TemperateDesert;
                if (moisture < 0.55) return BiomeType.Grassland;
                if (moisture < 0.8) return BiomeType.TemperateForest;
                return BiomeType.TemperateRainForest;
            }

            if (moisture < 0.2) return BiomeType.SubtropicalDesert;
            if (moisture < 0.4) return BiomeType.Grassland;
            if (moisture < 0.7) return BiomeType.TropicalForest;
            return BiomeType.TropicalRainForest;
        }

        class NoiseData
        {
            public readonly float[] Noisemap;
            public readonly Func<float, float, float> NoiseFunc;

            public NoiseData(float[] noiseMap, Func<float, float, float> noiseFunc)
            {
                Noisemap = noiseMap;
                NoiseFunc = noiseFunc;
            }
        }
    }
}
