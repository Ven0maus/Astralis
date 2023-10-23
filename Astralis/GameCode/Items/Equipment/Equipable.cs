using Astralis.GameCode.Items.Equipables;
using Astralis.GameCode.Npcs;

namespace Astralis.GameCode.Items.Equipment
{
    internal abstract class Equipable : Item, IEquipable
    {
        public abstract EquipableType Type { get; }
        public abstract void AddStats(Actor actor);
        public abstract void RemoveStats(Actor actor);
    }
}
