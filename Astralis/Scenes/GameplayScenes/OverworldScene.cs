using Astralis.Extended.Effects;
using SadConsole;
using SadRogue.Primitives;
using System;
using Map = Astralis.GameCode.Map;

namespace Astralis.Scenes.GameplayScenes
{
    internal class OverworldScene : Scene
    {
        private readonly Map _map;

        public OverworldScene()
        {
            RemoveControlLayer();

            _map = GenerateOverworld();

            if (!Constants.DebugMode)
            {
                var fadeEffect = new FadeEffect(Surface, TimeSpan.FromSeconds(2), FadeEffect.FadeMode.FadeIn, false);
                fadeEffect.OnFinished += GameStart;
                Effects.Add(fadeEffect);
            }
            else
            {
                GameStart();
            }
        }

        private void GameStart()
        {

        }

        private Map GenerateOverworld()
        {
            var map = new Map(Width, Height);

            // Initial draw
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var tile = map[x, y];
                    var foreground = !Constants.DebugMode ? tile.Foreground.SetAlpha(0) : tile.Foreground;
                    var background = !Constants.DebugMode ? tile.Background.SetAlpha(0) : tile.Background;
                    Surface.SetGlyph(x, y, tile.BlocksView ? '#' : 0, foreground, background);
                }
            }

            return map;
        }
    }
}
