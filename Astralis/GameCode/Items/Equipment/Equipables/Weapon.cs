using Astralis.GameCode.Items.Equipables;
using Astralis.GameCode.Npcs;

namespace Astralis.GameCode.Items.Equipment.Equipables
{
    internal class Weapon : Item, IEquipable
    {
        public EquipableType Type => EquipableType.Weapon;

        public void AddStats(Actor actor)
        {

        }

        public void RemoveStats(Actor actor)
        {

        }
    }
}
