using Astralis.GameCode.Items.Equipables;

namespace Astralis.GameCode.Items.Equipment.Equipables
{
    internal class Shield : Item, IEquipable
    {
        public EquipableType Type => EquipableType.Shield;
    }
}
