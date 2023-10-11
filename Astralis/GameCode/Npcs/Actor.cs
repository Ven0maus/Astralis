using SadConsole;
using SadConsole.Entities;

namespace Astralis.GameCode.Npcs
{
    internal class Actor : Entity
    {
        public Facing Facing { get; private init; }
        public Gender Gender { get; private init; }
        public Race Race { get; private init; }
        public Class Class { get; private init; }

        public Actor(ColoredGlyphBase appearance, int zIndex)
            : base(appearance, zIndex)
        { }
    }
}
