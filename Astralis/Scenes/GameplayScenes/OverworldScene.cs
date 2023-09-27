using Astralis.Extended.Effects;
using Astralis.GameCode;
using System;

namespace Astralis.Scenes.GameplayScenes
{
    internal class OverworldScene : Scene
    {
        private readonly World _world;
        private readonly WorldScreen _worldScreen;

        public OverworldScene()
        {
            // Generate world
            var seed = new Random().Next(-1000000, 1000000);
            _world = new World(Constants.ScreenWidth, Constants.ScreenHeight, seed, new WorldGenerator(seed));

            // Create world renderer
            _worldScreen = new WorldScreen(_world);
            Children.Add(_worldScreen);

            // Center the camera
            _world.Center(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
            _world.Center((Constants.ScreenWidth / 2) + 1, (Constants.ScreenHeight / 2) - 1);

            if (!Constants.DebugMode)
            {
                var fadeEffect = new FadeEffect(_worldScreen, TimeSpan.FromSeconds(2), FadeEffect.FadeMode.FadeIn, false);
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
    }
}
