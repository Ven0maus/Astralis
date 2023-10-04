using Astralis.Configuration.JsonHelpers;
using Astralis.GameCode.WorldGen;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Configuration.Models
{
    internal class BiomeGeneration
    {
        private Cache _cache;
        public Cache Get { get { return _cache ??= new Cache(this); } }

        // Data types for JSON
        public Biome[] Biomes { get; set; }

        public class Cache
        {
            private readonly BiomeGeneration _worldGeneration;
            public Dictionary<BiomeType, Biome> Biomes { get; private set; }

            public Cache(BiomeGeneration worldGeneration)
            {
                _worldGeneration = worldGeneration;

                InitBiomesCache();
            }

            private void InitBiomesCache()
            {
                // Set biome parent
                foreach (var biome in _worldGeneration.Biomes)
                {
                    if (biome.Objects != null)
                    {
                        foreach (var obj in biome.Objects)
                            obj.Biome = biome;
                    }
                }

                var tileTypes = ((BiomeType[])Enum.GetValues(typeof(BiomeType)))
                    .ToDictionary(a => Enum.GetName(a), a => a);
                Biomes = _worldGeneration.Biomes.Select(a =>
                {
                    return new { Biome = a, TileType = tileTypes[a.Name] };
                }).ToDictionary(a => a.TileType, a => a.Biome);
            }
        }

        internal class Biome
        {
            public string Name { get; set; }

            public BiomeObject[] Objects { get; set; }

            [JsonConverter(typeof(ColorConverter))]
            public Color Color { get; set; }
        }

        internal class BiomeObject
        {
            [JsonIgnore]
            public Biome Biome { get; set; }

            public string Name { get; set; }
            public int SpawnChance { get; set; }
            public Rarity SpawnRarity { get; set; }
            public bool? CopyBiomeColor { get; set; }
            public int? MinNeighborsGrowth { get; set; }
            public int? MinNeighborsSurvival { get; set; }

            public WorldObject WorldObject
            {
                get
                {
                    return World.ObjectData.Get.Objects.TryGetValue(Name, out var value) ? value :
                        throw new Exception("Missing world object configuration for biome object: " + Name);
                }
            }
        }

        internal enum Rarity
        {
            Normal,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythical
        }
    }
}
