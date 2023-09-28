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
            public Dictionary<ObjectType, WorldObject> Objects { get; private set; }

            public Cache(WorldObjects worldObjects)
            {
                _worldObjects = worldObjects;

                InitObjectsCache();
            }

            private void InitObjectsCache()
            {
                var objectTypes = ((ObjectType[])Enum.GetValues(typeof(ObjectType)))
                    .ToDictionary(a => Enum.GetName(a), a => a);
                Objects = _worldObjects.Objects.Select(a =>
                {
                    return new { Object = a, ObjectType = objectTypes[a.Name] };
                }).ToDictionary(a => a.ObjectType, a => a.Object);
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
