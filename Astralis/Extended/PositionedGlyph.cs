using SadConsole;
using SadRogue.Primitives;

namespace Astralis.Extended
{
    public class PositionedGlyph
    {
        public Point Destination { get; }
        public Point? Previous { get; private set; }
        public Point Current { get; private set; }
        public Point Start { get; private set; }
        public ColoredGlyph Glyph { get; }

        private bool _init = false;

        public PositionedGlyph(ColoredGlyph glyph, Point position)
        {
            Destination = position;
            Glyph = glyph;
        }

        public void Init(Point startPosition)
        {
            if (_init) return;
            Start = startPosition;
            Current = Start;
            _init = true;
        }

        public void UpdatePosition(Point previous, Point current)
        {
            Previous = previous;
            Current = current;
        }
    }
}
