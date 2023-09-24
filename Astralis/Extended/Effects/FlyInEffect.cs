using Astralis.Extended.Effects.Core;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;

namespace Astralis.Extended.Effects
{
    internal class FlyInEffect : IEffect
    {
        private readonly Point _destinationPoint;
        private readonly TimeSpan _duration;
        private readonly TimeSpan _startAfter;
        private readonly Action<Point> _callBack;

        private DateTime _startTime;
        private Point _objectPosition;
        private bool _hasStarted = false;

        public bool IsFinished { get; private set; } = false;
        public Action OnFinished { get; set; }

        /// <summary>
        /// Constructor for FlyInEffect
        /// </summary>
        /// <param name="start">Starting point</param>
        /// <param name="destination">Destination point</param>
        /// <param name="duration">Duration to reach the destination</param>
        /// <param name="callBack">Called during each update step with the updated position</param>
        /// <param name="startAfter">Start the effect after x delay</param>
        public FlyInEffect(Point start, Point destination, TimeSpan duration, Action<Point> callBack, TimeSpan? startAfter = null)
        {
            _startAfter = startAfter ?? TimeSpan.Zero;
            _objectPosition = start;
            _destinationPoint = destination;
            _duration = duration;
            _startTime = DateTime.Now;
            _callBack = callBack;
        }

        public static FlyInEffect Create(ControlBase so, Direction flyInDirection, TimeSpan duration, TimeSpan? startAfter = null)
        {
            if (so.Parent == null) 
                throw new Exception("Control is not part of a ControlHost.");

            int height = 0, width = 0;
            var parent = (ScreenSurface)((ControlHost)so.Parent).ParentConsole;
            height = parent.Height;
            width = parent.Width;

            startAfter ??= TimeSpan.Zero;
            var destination = so.Position;
            var start = so.Position;

            switch (flyInDirection)
            {
                case Direction.Bottom:
                    start = new Point(so.Position.X, height + so.Height);
                    break;
                case Direction.Top:
                    start = new Point(so.Position.X, 0 - so.Height);
                    break;
                case Direction.Left:
                    start = new Point(0 - so.Width, so.Position.Y);
                    break;
                case Direction.Right:
                    start = new Point(width + so.Width, so.Position.Y);
                    break;
            }

            so.Position = start;

            return new FlyInEffect(start, destination, duration, (pos) => { so.Position = pos; so.IsDirty = true; }, startAfter);
        }

        public void Update()
        {
            TimeSpan elapsedTime = DateTime.Now - _startTime;
            if (!_hasStarted && elapsedTime < _startAfter) return;

            if (!_hasStarted)
            {
                _hasStarted = true;
                _startTime = DateTime.Now;
                elapsedTime = DateTime.Now - _startTime;
            }

            // Calculate the normalized progress (0 to 1) of the movement.
            double normalizedProgress = Math.Clamp(elapsedTime.TotalMilliseconds / _duration.TotalMilliseconds, 0.0, 1.0);

            // Interpolate between the initial and destination positions.
            Point newPosition = new Point(
                (int)(_objectPosition.X + (_destinationPoint.X - _objectPosition.X) * normalizedProgress),
                (int)(_objectPosition.Y + (_destinationPoint.Y - _objectPosition.Y) * normalizedProgress)
            );

            // If the normalized progress reaches 1, the movement is complete.
            if (normalizedProgress >= 1.0)
            {
                IsFinished = true;
                newPosition = _destinationPoint;
            }

            // Update the object's position.
            _callBack(newPosition);
        }

        public enum Direction
        {
            Bottom,
            Top,
            Left,
            Right
        }
    }
}
