using Astralis.Extended.Effects.Core;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Extended.Effects
{
    internal class MovingTextEffect : IEffect
    {
        public bool IsFinished { get; private set; }
        public Action OnFinished { get; set; }

        protected readonly PositionedGlyph[] _positionedGlyphs;
        protected readonly ScreenSurface _surface;
        private readonly TimeSpan _duration;
        private readonly TimeSpan _startAfter;

        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private bool _hasStarted = false;

        public MovingTextEffect(IEnumerable<PositionedGlyph> positionedGlyphs, ScreenSurface surface, TimeSpan duration, TimeSpan? startAfter = null)
        {
            _positionedGlyphs = positionedGlyphs.ToArray();
            _surface = surface;
            _duration = duration;
            _startAfter = startAfter ?? TimeSpan.Zero;
        }

        public void Update(TimeSpan delta)
        {
            _elapsedTime += delta;

            if (!_hasStarted && _elapsedTime < _startAfter) return;

            if (!_hasStarted)
            {
                _hasStarted = true;
                _elapsedTime = TimeSpan.Zero + delta;
            }

            // Calculate the normalized progress (0 to 1) of the movement.
            double normalizedProgress = Math.Clamp(_elapsedTime.TotalMilliseconds / _duration.TotalMilliseconds, 0.0, 1.0);

            foreach (var positionedGlyph in _positionedGlyphs)
            {
                // Interpolate between the initial and destination positions.
                Point newPosition = new Point(
                    (int)(positionedGlyph.Start.X + (positionedGlyph.Destination.X - positionedGlyph.Start.X) * normalizedProgress),
                    (int)(positionedGlyph.Start.Y + (positionedGlyph.Destination.Y - positionedGlyph.Start.Y) * normalizedProgress)
                );

                // If the normalized progress reaches 1, the movement is complete.
                if (normalizedProgress >= 1.0)
                    newPosition = positionedGlyph.Destination;

                positionedGlyph.UpdatePosition(positionedGlyph.Current, newPosition);
            }

            DrawPositionedGlyphs();

            if (normalizedProgress >= 1.0)
            {
                IsFinished = true;
            }
        }

        protected void DrawPositionedGlyphs()
        {
            foreach (var positionedGlyph in _positionedGlyphs)
            {
                // Clear the previous glyph if no other glyph is currently on this position
                if (positionedGlyph.Previous.HasValue && !PositionOccupied(positionedGlyph.Previous.Value))
                    _surface.Clear(positionedGlyph.Previous.Value.X, positionedGlyph.Previous.Value.Y);
                if (_surface.Surface.IsValidCell(positionedGlyph.Current.X, positionedGlyph.Current.Y))
                    _surface.Surface[positionedGlyph.Current.X, positionedGlyph.Current.Y].CopyAppearanceFrom(positionedGlyph.Glyph, false);
            }
        }

        private bool PositionOccupied(Point position)
        {
            return _positionedGlyphs.Any(a => a.Current.Equals(position));
        }
    }
}
