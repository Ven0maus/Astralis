using Astralis.Extended.Effects.Core;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using Venomaus.FlowVitae.Helpers;

namespace Astralis.Extended.Effects
{
    internal class FadeEffect : IEffect
    {
        public bool IsFinished { get; private set; }
        public Action OnFinished { get; set; }

        private readonly ScreenSurface[] _surfaces;
        private readonly TimeSpan _fadeDuration;
        private readonly bool _loop;
        private readonly FadeContext _fadeContext;

        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private bool _fadeOut;

        private readonly Dictionary<ScreenSurface, Dictionary<(int x, int y), (byte foreground, byte background)>> _startAlphas;
        private readonly TupleComparer<int> _comparer;

        public FadeEffect(TimeSpan fadeDuration, FadeContext fadeContext, FadeMode fadeMode, bool loop, params ScreenSurface[] surfaces)
        {
            _surfaces = surfaces;
            _fadeOut = fadeMode == FadeMode.FadeOut;
            _fadeContext = fadeContext;
            _loop = loop;
            _fadeDuration = fadeDuration;
            _comparer = new TupleComparer<int>();
            _startAlphas = new Dictionary<ScreenSurface, Dictionary<(int x, int y), (byte foreground, byte background)>>();

            // Store original alpha values before starting the fade
            StoreOriginalAlphas();

            // Initialize glyphs with the current alpha values
            SetGlyphsAlpha(_fadeOut ? 1 : 0);
        }

        private void StoreOriginalAlphas()
        {
            foreach (var surface in _surfaces)
            {
                for (int x = 0; x < surface.Width; x++)
                {
                    for (int y = 0; y < surface.Height; y++)
                    {
                        var foreground = surface.Surface[x, y].Foreground;
                        var background = surface.Surface[x, y].Background;

                        // Store original alpha values
                        if (!_startAlphas.TryGetValue(surface, out var alphas))
                        {
                            _startAlphas.Add(surface, alphas = new Dictionary<(int x, int y), (byte, byte)>(_comparer));
                        }
                        alphas[(x, y)] = (foreground.A, background.A);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the foreground alpha on each glyph on the surface that has a valid glyph
        /// </summary>
        /// <param name="alpha">0.0 - 1.0 where 1 is visible</param>
        private void SetGlyphsAlpha(double alpha)
        {
            foreach (var surface in _surfaces)
            {
                for (int x = 0; x < surface.Width; x++)
                {
                    for (int y = 0; y < surface.Height; y++)
                    {
                        var foreground = surface.Surface[x, y].Foreground;
                        var background = surface.Surface[x, y].Background;

                        // Get original alpha values
                        var originalAlphas = _startAlphas[surface][(x, y)];
                        var clampedValue = ClampTo0_255(alpha);

                        if (_fadeOut)
                        {
                            if (clampedValue <= originalAlphas.foreground && foreground != Color.Transparent && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Foreground)
                                surface.Surface[x, y].Foreground = foreground.SetAlpha(clampedValue);
                            if (clampedValue <= originalAlphas.background && background != Color.Transparent && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Background)
                                surface.Surface[x, y].Background = background.SetAlpha(clampedValue);
                            if (clampedValue <= originalAlphas.foreground && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Foreground)
                                surface.Surface[x, y].Decorators = AdjustDecoratorsColor(surface.Surface[x, y].Decorators, clampedValue);
                        }
                        else
                        {
                            if (clampedValue <= originalAlphas.foreground && foreground != Color.Transparent && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Foreground)
                                surface.Surface[x, y].Foreground = foreground.SetAlpha(clampedValue);
                            if (clampedValue <= originalAlphas.background && background != Color.Transparent && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Background)
                                surface.Surface[x, y].Background = background.SetAlpha(clampedValue);
                            if (clampedValue <= originalAlphas.foreground && _fadeContext == FadeContext.Both || _fadeContext == FadeContext.Foreground)
                                surface.Surface[x, y].Decorators = AdjustDecoratorsColor(surface.Surface[x, y].Decorators, clampedValue);
                        }
                    }
                }
                surface.IsDirty = true;
            }
        }

        public enum FadeMode
        {
            FadeIn,
            FadeOut,
        }

        public enum FadeContext
        {
            Foreground,
            Background,
            Both
        }

        private static byte ClampTo0_255(double value)
        {
            double scaledValue = value * 255.0;
            int roundedValue = (int)Math.Round(scaledValue);
            byte clampedValue = (byte)Math.Max(0, Math.Min(255, roundedValue));
            return clampedValue;
        }

        private static List<CellDecorator> AdjustDecoratorsColor(List<CellDecorator> decorators, byte alpha)
        {
            if (decorators == null || decorators.Count == 0) return decorators;
            for (int i = 0; i < decorators.Count; i++)
            {
                var dec = decorators[i];
                if (dec.Color == Color.Transparent) continue;
                decorators[i] = new CellDecorator(dec.Color.SetAlpha(alpha), dec.Glyph, dec.Mirror);
            }
            return decorators;
        }

        private void FadeOut(double alpha)
        {
            SetGlyphsAlpha(1d - alpha);
        }

        private void FadeIn(double alpha)
        {
            SetGlyphsAlpha(alpha);
        }

        public void Update(TimeSpan delta)
        {
            _elapsedTime += delta;

            // Calculate the normalized progress (0 to 1) of the movement.
            double normalizedProgress = Math.Clamp(_elapsedTime.TotalMilliseconds / _fadeDuration.TotalMilliseconds, 0.0, 1.0);
            if (normalizedProgress >= 1.0)
                normalizedProgress = 1d;

            if (_fadeOut)
                FadeOut(normalizedProgress);
            else
                FadeIn(normalizedProgress);

            if (normalizedProgress >= 1.0)
            {
                _fadeOut = !_fadeOut;
                _elapsedTime = TimeSpan.Zero;
                if (!_loop)
                    IsFinished = true;
            }
        }
    }
}
