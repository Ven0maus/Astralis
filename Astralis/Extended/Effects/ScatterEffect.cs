using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Extended.Effects
{
    internal class ScatterEffect : IEffect
    {
        public bool IsFinished { get; private set; }

        public Action OnFinished { get; set; }

        private readonly ScatterGlyph[] _scatterGlyphs;
        private readonly int _width, _height;
        private readonly ScreenSurface _surface;
        private readonly HashSet<Point> _positions;
        private readonly TimeSpan _duration;
        private readonly DateTime _startTime;

        public ScatterEffect(IEnumerable<ScatterGlyph> glyphs, ScreenSurface surface, TimeSpan duration) 
        {
            _positions = new HashSet<Point>();
            _scatterGlyphs = glyphs.ToArray();
            _duration = duration;
            _surface = surface;
            _width = surface.Width;
            _height = surface.Height;

            Scatter();

            _startTime = DateTime.Now;
        }

        private void Scatter()
        {
            var random = new Random();

            foreach (var scatterGlyph in _scatterGlyphs)
            {
                var point = new Point(random.Next(0, _width), random.Next(0, _height));
                while (_positions.Contains(point))
                    point = new Point(random.Next(0, _width), random.Next(0, _height));

                scatterGlyph.Init(point);
                _positions.Add(point);
            }

            DrawScatteredGlyphs();
        }

        public void Update()
        {
            var elapsedTime = DateTime.Now - _startTime;

            // Calculate the normalized progress (0 to 1) of the movement.
            double normalizedProgress = Math.Clamp(elapsedTime.TotalMilliseconds / _duration.TotalMilliseconds, 0.0, 1.0);

            foreach (var scatterGlyph in _scatterGlyphs)
            {
                // Interpolate between the initial and destination positions.
                Point newPosition = new Point(
                    (int)(scatterGlyph.Start.X + (scatterGlyph.Destination.X - scatterGlyph.Start.X) * normalizedProgress),
                    (int)(scatterGlyph.Start.Y + (scatterGlyph.Destination.Y - scatterGlyph.Start.Y) * normalizedProgress)
                );

                // If the normalized progress reaches 1, the movement is complete.
                if (normalizedProgress >= 1.0)
                    newPosition = scatterGlyph.Destination;

                scatterGlyph.UpdatePosition(scatterGlyph.Current, newPosition);
            }

            DrawScatteredGlyphs();

            if (normalizedProgress >= 1.0)
            {
                IsFinished = true;
            }
        }

        private void DrawScatteredGlyphs()
        {
            foreach (var scatteredGlyph in _scatterGlyphs)
            {
                // Clear the previous glyph if no other glyph is currently on this position
                if (scatteredGlyph.Previous.HasValue && !PositionOccupied(scatteredGlyph.Previous.Value))
                    _surface.Clear(scatteredGlyph.Previous.Value.X, scatteredGlyph.Previous.Value.Y);
                _surface.Surface[scatteredGlyph.Current.X, scatteredGlyph.Current.Y].CopyAppearanceFrom(scatteredGlyph.Glyph, false);
            }
        }

        private bool PositionOccupied(Point position)
        {
            return _scatterGlyphs.Any(a => a.Current.Equals(position));
        }

        public class ScatterGlyph
        {
            public Point Destination { get; }
            public Point? Previous { get; private set; }
            public Point Current { get; private set; }
            public Point Start { get; private set; }
            public ColoredGlyph Glyph { get; }

            private bool _init = false;

            public ScatterGlyph(ColoredGlyph glyph, Point position)
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
}
