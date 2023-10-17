using Astralis.GameCode.Items.Equipables;
using Astralis.GameCode.Npcs;

namespace Astralis.GameCode.Items.Equipment
{
    internal abstract class Armour : Item, IEquipable
    {
        public abstract EquipableType Type { get; }

        public void AddStats(Actor actor)
        {

        }

        public void RemoveStats(Actor actor)
        {

        }
    }
}
