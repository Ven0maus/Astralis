using Astralis.Configuration.JsonHelpers;
using Astralis.GameCode;
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
            public string Name { get; set; }
            public int SpawnChance { get; set; }
            public Rarity Rarity { get; set; }
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
