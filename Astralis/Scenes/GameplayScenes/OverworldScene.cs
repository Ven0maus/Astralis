using Astralis.Extended.Effects;
using Astralis.GameCode.WorldGen;
using Astralis.Scenes.Screens;
using System;

namespace Astralis.Scenes.GameplayScenes
{
    internal class OverworldScene : Scene
    {
        private readonly World _world;
        private readonly WorldScreen _worldScreen;

        public event EventHandler OnMainMenuVisualLoaded;

        public OverworldScene()
        {
            // Generate world
            var seed = Constants.DebugMode ? Constants.GameSeed : new Random().Next(-1000000, 1000000);
            var worldGenerator = new WorldGenerator(seed, new Extended.NoiseHelper(seed));
            var chunkSize = Constants.WorldGeneration.ChunkSize;
            _world = new World(Constants.ScreenWidth, Constants.ScreenHeight, chunkSize, chunkSize, worldGenerator);

            // Create world renderer
            _worldScreen = new WorldScreen(_world);
            Children.Add(_worldScreen);

            // Center the camera
            _world.Center(Constants.ScreenWidth / 2, Constants.ScreenHeight / 2);
            _world.Center((Constants.ScreenWidth / 2) + 1, (Constants.ScreenHeight / 2) - 1);
        }

        public void Initialize(bool mainMenuVisuals)
        {
            if (mainMenuVisuals)
            {
                var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(1), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _worldScreen.GetSurfaces());
                fadeEffect.OnFinished += () => { _worldScreen.MainMenuCamera = mainMenuVisuals; OnMainMenuVisualLoaded?.Invoke(null, null); };
                Effects.Add(fadeEffect);
            }
            else
            {
                if (!Constants.DebugMode)
                {
                    var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(2), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _worldScreen.GetSurfaces());
                    fadeEffect.OnFinished += GameStart;
                    Effects.Add(fadeEffect);
                }
                else
                {
                    GameStart();
                }
            }
        }

        public void DeintializeMainMenuVisuals()
        {
            _worldScreen.MainMenuCamera = false;
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(2), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeOut, false, _worldScreen.GetSurfaces());
            fadeEffect.OnFinished += () => { OnMainMenuVisualLoaded?.Invoke(null, null); };
            Effects.Add(fadeEffect);
        }

        private void GameStart()
        {

        }
    }
}
