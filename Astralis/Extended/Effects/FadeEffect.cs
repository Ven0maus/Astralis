using Astralis.Extended.Effects.Core;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Astralis.Extended.Effects
{
    internal class FadeEffect : IEffect
    {
        public bool IsFinished { get; private set; }
        public Action OnFinished { get; set; }

        private readonly ScreenSurface _surface;
        private readonly TimeSpan _fadeDuration;
        private readonly FadeMode _fadeMode;
        private readonly bool _loop;

        private DateTime _startTime;
        private bool _fadeOut;

        public FadeEffect(ScreenSurface surface, TimeSpan fadeDuration, FadeMode fadeMode, bool loop = false)
        {
            _surface = surface;
            _fadeMode = fadeMode;
            _fadeOut = fadeMode == FadeMode.FadeOut;
            _loop = loop;
            _fadeDuration = fadeDuration;
            _startTime = DateTime.Now;

            SetGlyphsAlpha(_fadeOut ? 1 : 0);
        }

        public enum FadeMode
        {
            FadeIn,
            FadeOut,
        }

        /// <summary>
        /// Sets the foreground alpha on each glyph on the surface that has a valid glyph
        /// </summary>
        /// <param name="alpha">0.0 - 1.0 where 1 is invisible</param>
        private void SetGlyphsAlpha(double alpha)
        {
            for (int x = 0; x < _surface.Width; x++)
            {
                for (int y = 0; y < _surface.Height; y++)
                {
                    var foreground = _surface.Surface[x, y].Foreground;
                    var background = _surface.Surface[x, y].Background;
                    _surface.Surface[x, y].Foreground = foreground.SetAlpha(ClampTo0_255(alpha));
                    _surface.Surface[x, y].Background = background.SetAlpha(ClampTo0_255(alpha));
                }
            }
            _surface.IsDirty = true;
        }

        private static byte ClampTo0_255(double value)
        {
            double scaledValue = value * 255.0;
            int roundedValue = (int)Math.Round(scaledValue);
            byte clampedValue = (byte)Math.Max(0, Math.Min(255, roundedValue));
            return clampedValue;
        }

        private void FadeOut(double alpha)
        {
            SetGlyphsAlpha(1d - alpha);
        }

        private void FadeIn(double alpha)
        {
            SetGlyphsAlpha(alpha);
        }

        public void Update()
        {
            var elapsedTime = DateTime.Now - _startTime;

            // Calculate the normalized progress (0 to 1) of the movement.
            double normalizedProgress = Math.Clamp(elapsedTime.TotalMilliseconds / _fadeDuration.TotalMilliseconds, 0.0, 1.0);
            if (normalizedProgress >= 1.0)
                normalizedProgress = 1d;

            if (_fadeOut)
                FadeOut(normalizedProgress);
            else
                FadeIn(normalizedProgress);

            if (normalizedProgress >= 1.0)
            {
                _fadeOut = !_fadeOut;
                _startTime = DateTime.Now;
                if (!_loop)
                    IsFinished = true;
            }
        }
    }
}
