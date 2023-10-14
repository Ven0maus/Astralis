using System.Collections.Generic;
using System.Linq;

namespace Astralis.Configuration.Models
{
    internal class NpcTraits
    {
        private Cache _cache;
        public Cache Get { get { return _cache ??= new Cache(this); } }

        public NpcTrait[] Traits { get; set; }
        public NpcTraitPreset[] TraitPresets { get; set; }

        public class Cache
        {
            private readonly NpcTraits _npcTraits;
            public Dictionary<string, NpcTrait> NpcTraits { get; private set; }
            public Dictionary<string, NpcTraitPreset> NpcTraitPresets { get; private set; }

            public Cache(NpcTraits npcTraits)
            {
                _npcTraits = npcTraits;

                InitNpcTraitsCache();
            }

            private void InitNpcTraitsCache()
            {
                NpcTraits = _npcTraits.Traits.ToDictionary(a => a.Name, a => a);
                NpcTraitPresets = _npcTraits.TraitPresets.ToDictionary(a => a.Name, a => a);
            }
        }
    }

    internal class NpcTrait
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public string[] Counters { get; set; }
    }

    internal class NpcTraitPreset
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Traits { get; set; }
    }
}
