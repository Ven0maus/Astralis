using Astralis.Configuration.Models;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.GameCode.WorldGen
{
    internal class CellularAutomaton
    {
        public static void Apply(BiomeGeneration.BiomeObject[] objects, byte[] biomes, int width, int height, int iterations)
        {
            if (iterations <= 0)
                iterations = 1;

            var original = new BiomeGeneration.BiomeObject[objects.Length];
            Copy(objects, original, width, height);

            var objectLookup = World.ObjectData.Objects.ToDictionary(a => a.Id, a => a);

            // Iterate over the cells and apply the automaton rules based on the biome and object preferences.
            for (int i = 0; i < iterations; i++)
            {
                var newGrid = new BiomeGeneration.BiomeObject[original.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var index = y * width + x;

                        if (!World.BiomeData.Get.Biomes.TryGetValue((BiomeType)biomes[index], out var biome))
                            throw new System.Exception("No biome for this index??");

                        // If this biome doesn't support objects, we don't need to process it
                        if (biome.Objects == null) continue;

                        var neighbors = GetNeighborsOfSameBiome(original, x, y, width, height, true, biomes, biomes[index]).ToArray();
                        int neighborCount = 0;

                        var obj = original[index];
                        if (obj == null)
                        {
                            // Find the object that is most common in the neighbors
                            var neighborType = GetHighestNeighborType(neighbors, out neighborCount);
                            if (neighborCount > 0)
                            {
                                obj = biome.Objects.First(a => a.WorldObject.Id == neighborType);
                            }
                            else
                            {
                                // No objects in neighbors, we skip this one
                                continue;
                            }
                        }

                        // Check if the object at this location should undergo cellular automaton.
                        if (ShouldApplyAutomaton(obj))
                        {
                            if (neighborCount == 0)
                                neighborCount = neighbors.Count(a => a != null && a.WorldObject.Id == obj.WorldObject.Id);

                            // Apply cellular automaton rules based on the object's preferences.
                            if (original[index] == null && obj.MinNeighborsGrowth.HasValue &&
                                neighborCount >= obj.MinNeighborsGrowth.Value)
                            {
                                // Growth logic
                                newGrid[index] = obj;
                            }
                            else if (original[index] != null && obj.MinNeighborsSurvival.HasValue &&
                                     neighborCount < obj.MinNeighborsSurvival.Value)
                            {
                                // Survival logic.
                                newGrid[index] = null;
                            }
                            else
                            {
                                // Leave as is
                                newGrid[index] = original[index];
                            }
                        }
                        else
                        {
                            // Leave as is
                            newGrid[index] = original[index];
                        }
                    }
                }
                original = newGrid;
            }
            Copy(original, objects, width, height);
        }

        private static void Copy<T>(T[] fromArray, T[] toArray, int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    toArray[y * width + x] = fromArray[y * width + x];
                }
            }
        }

        private static byte GetHighestNeighborType(BiomeGeneration.BiomeObject[] neighbors, out int amount)
        {
            var neighborCounts = new Dictionary<byte, int>();
            foreach (var neighbor in neighbors.Where(a => a != null && a.WorldObject.Id != 0))
            {
                if (neighborCounts.ContainsKey(neighbor.WorldObject.Id))
                {
                    neighborCounts[neighbor.WorldObject.Id]++;
                }
                else
                {
                    neighborCounts[neighbor.WorldObject.Id] = 1;
                }
            }

            if (neighborCounts.Count > 0)
            {
                var keyWithHighestCount = neighborCounts.OrderByDescending(kv => kv.Value).First().Key;
                amount = neighborCounts[keyWithHighestCount];
                return keyWithHighestCount;
            }

            amount = 0;
            return 0;
        }

        private static bool ShouldApplyAutomaton(BiomeGeneration.BiomeObject biomeObject)
        {
            return biomeObject.MinNeighborsSurvival != null || biomeObject.MinNeighborsGrowth != null;
        }

        private static IEnumerable<T> GetNeighborsOfSameBiome<T>(T[] grid, int x, int y, int width, int height, bool includeDiagonals, byte[] biomes, byte biome)
        {
            int[] dx = includeDiagonals ? new int[] { -1, 1, 0, 0, -1, 1, -1, 1 } : new int[] { -1, 1, 0, 0 };
            int[] dy = includeDiagonals ? new int[] { 0, 0, -1, 1, -1, -1, 1, 1 } : new int[] { 0, 0, -1, 1 };

            for (int i = 0; i < dx.Length; i++)
            {
                int newX = x + dx[i];
                int newY = y + dy[i];

                if (InBounds(newX, newY, width, height) && biomes[newY * width + newX] == biome)
                {
                    yield return grid[newY * width + newX];
                }
            }
        }

        private static bool InBounds(int x, int y, int width, int height)
        {
            return x >= 0 && y >= 0 && x < width && y < height;
        }
    }
}
