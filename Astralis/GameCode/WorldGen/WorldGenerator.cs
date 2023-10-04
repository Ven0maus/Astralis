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

        // Global values used to approximate chunk remapping
        // This requires beforehand a bunch of chunks to be generated,
        // So the global values can be computed
        public static float GlobalMin, GlobalMax;

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
            var heat = new float[width * height];
            var rivers = new float[width * height];

            SetNoisemap(width, height, chunkCoordinate,
                new NoiseData(elevation, NoiseHelper.GetElevation),
                new NoiseData(moisture, NoiseHelper.GetMoisture),
                new NoiseData(heat, NoiseHelper.GetHeat),
                new NoiseData(rivers, NoiseHelper.GetRivers));

            var combinedMap = CombineMaps(width, height, elevation, moisture, heat);

            // Set chunk based on provided lookup arrays
            var biomes = new byte[width * height];
            var objects = new BiomeGeneration.BiomeObject[width * height];
            SetChunkValues(random, biomes, objects, width, height, combinedMap, rivers);

            return (biomes, new WorldChunk(biomes, objects, width, height, random));
        }

        private static float[] CombineMaps(int width, int height, float[] elevationMap, float[] moistureMap, float[] heatMap)
        {
            float[] combinedMap = new float[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    var value = elevationMap[index] + moistureMap[index] + heatMap[index];
                    combinedMap[index] = value;

                    GlobalMin = Math.Min(GlobalMin, value);
                    GlobalMax = Math.Max(GlobalMax, value);
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    combinedMap[index] = Mathf.Remap(combinedMap[index], GlobalMin, GlobalMax, 0f, 1f);
                }
            }
            return combinedMap;
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

        private static void SetChunkValues(Random random, byte[] biomes, BiomeGeneration.BiomeObject[] objects,
            int width, int height, float[] noise, float[] rivers)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var index = y * width + x;
                    if ((x == 0 || y == 0 || x == width - 1 || y == height - 1) &&
                        Constants.DebugMode && Constants.WorldGeneration.DrawBordersOnDebugMode)
                    {
                        biomes[index] = (byte)BiomeType.Border;
                        continue;
                    }

                    var biomeType = GetTileType(noise[index], rivers[index]);
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

        public static BiomeType GetTileType(float noise, float riverNoise)
        {
            // Define biome thresholds based on the combined value
            if (noise < 0.15f) return BiomeType.Ocean;
            else if (riverNoise > 0.44f)
            {
                if (riverNoise > 0.47f)
                {
                    if (noise >= 0.92f)
                        return BiomeType.FrozenRiver;
                    return BiomeType.River;
                }
                if (noise >= 0.76f && noise < 0.92f)
                {
                    return BiomeType.Tundra;
                }
                return BiomeType.Beach;
            }
            else if (noise < 0.225f) return BiomeType.Beach;
            else if (noise < 0.27f) return BiomeType.SubtropicalDesert;
            else if (noise < 0.32f) return BiomeType.TemperateDesert;
            else if (noise < 0.43f) return BiomeType.Grassland;
            else if (noise < 0.51f) return BiomeType.TemperateForest;
            else if (noise < 0.57f) return BiomeType.TropicalForest;
            else if (noise < 0.62f) return BiomeType.TemperateRainForest;
            else if (noise < 0.72f) return BiomeType.TropicalRainForest;
            else if (noise < 0.76f) return BiomeType.Swamp;
            else if (noise < 0.86f) return BiomeType.Taiga;
            else if (noise < 0.92) return BiomeType.Tundra;
            return BiomeType.Snow;
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
