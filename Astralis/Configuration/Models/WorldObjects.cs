using Astralis.Configuration.JsonHelpers;
using Astralis.GameCode;
using Newtonsoft.Json;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Configuration.Models
{
    internal class WorldObjects
    {
        private Cache _cache;
        public Cache Get { get { return _cache ??= new Cache(this); } }

        public WorldObject[] Objects { get; set; }

        public class Cache
        {
            private readonly WorldObjects _worldObjects;
            public Dictionary<string, WorldObject> Objects { get; private set; }

            public Cache(WorldObjects worldObjects)
            {
                _worldObjects = worldObjects;

                InitObjectsCache();
            }

            public WorldObject GetCustomObject(CustomObject customObject)
            {
                return Objects.TryGetValue(Enum.GetName(customObject), out var value) ? value : null;
            }

            private void InitObjectsCache()
            {
                Objects = _worldObjects.Objects.ToDictionary(a => a.Name, a => a);
            }
        }
    }

    internal class WorldObject
    {
        public int Glyph { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(ColorConverter))]
        public Color Color { get; set; }
    }
}
