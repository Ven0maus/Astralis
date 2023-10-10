using Astralis.Extended.Effects.Core;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Astralis.Extended.Effects
{
    internal class ScatterEffect : MovingTextEffect
    {
        private readonly HashSet<Point> _positions;

        public ScatterEffect(IEnumerable<PositionedGlyph> positionedGlyphs, ScreenSurface surface, TimeSpan duration) :
            base(positionedGlyphs, surface, duration)
        {
            _positions = new HashSet<Point>();
            Scatter();
        }

        private void Scatter()
        {
            var random = Constants.Random;

            foreach (var scatterGlyph in _positionedGlyphs)
            {
                var point = new Point(random.Next(0, _surface.Width), random.Next(0, _surface.Height));
                while (_positions.Contains(point))
                    point = new Point(random.Next(0, _surface.Width), random.Next(0, _surface.Height));

                scatterGlyph.Init(point);
                _positions.Add(point);
            }

            DrawPositionedGlyphs();
        }
    }
}
