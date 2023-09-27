using Astralis.Configuration.JsonHelpers;
using Astralis.GameCode;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Configuration.Models
{
    internal class WorldGeneration
    {
        private Cache _cache;
        public Cache Get { get { return _cache ??= new Cache(this); } }

        // Data types for JSON
        public Biome[] Biomes { get; set; }

        public class Cache
        {
            private readonly WorldGeneration _worldGeneration;
            public Dictionary<TileType, Biome> Biomes { get; private set; }

            public Cache(WorldGeneration worldGeneration)
            {
                _worldGeneration = worldGeneration;

                InitBiomesCache();
            }

            private void InitBiomesCache()
            {
                var tileTypes = ((TileType[])Enum.GetValues(typeof(TileType)))
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

            [JsonConverter(typeof(ColorConverter))]
            public Color BackgroundColor { get; set; }
        }
    }
}
