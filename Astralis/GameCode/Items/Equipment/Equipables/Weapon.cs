using Astralis.GameCode.Items.Equipables;

namespace Astralis.GameCode.Items.Equipment.Equipables
{
    internal class Weapon : Item, IEquipable
    {
        public EquipableType Type => EquipableType.Weapon;
    }
}
