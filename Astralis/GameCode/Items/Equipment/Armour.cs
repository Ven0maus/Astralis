using Astralis.GameCode.Items.Equipables;

namespace Astralis.GameCode.Items.Equipment
{
    internal abstract class Armour : Item, IEquipable
    {
        public abstract EquipableType Type { get; }
    }
}
