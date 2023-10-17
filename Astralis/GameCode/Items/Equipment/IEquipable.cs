using Astralis.GameCode.Npcs;

namespace Astralis.GameCode.Items.Equipables
{
    internal interface IEquipable
    {
        EquipableType Type { get; }
        void AddStats(Actor actor);
        void RemoveStats(Actor actor);
    }
}
