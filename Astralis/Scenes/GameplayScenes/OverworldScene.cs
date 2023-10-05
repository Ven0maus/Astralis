using Astralis.Extended.Effects;
using Astralis.GameCode.WorldGen;
using Astralis.Scenes.Screens;
using System;

namespace Astralis.Scenes.GameplayScenes
{
    internal class OverworldScene : Scene
    {
        private World _world;
        private WorldScreen _worldScreen;

        /// <summary>
        /// Used to push information between the mainmenu if this overworld is used as a background visual for it.
        /// </summary>
        public event EventHandler MainMenuCallBack;

        public World World { get { return _world; } }

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

        ~OverworldScene()
        {
            Dispose();
        }

        public void StartGame()
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

        private void GameStart()
        {

        }

        public void FadeInMainMenu()
        {
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(1), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _worldScreen.GetSurfaces())
            {
                OnFinished = () => 
                { 
                    _worldScreen.MainMenuCamera = true; 
                    MainMenuCallBack?.Invoke(null, null); 
                }
            };
            Effects.Add(fadeEffect);
        }

        public void FadeOutMainMenu()
        {
            _worldScreen.MainMenuCamera = false;
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(2), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeOut, false, _worldScreen.GetSurfaces())
            {
                OnFinished = () => 
                { 
                    MainMenuCallBack?.Invoke(null, null); 
                }
            };
            Effects.Add(fadeEffect);
        }

        public override void Dispose()
        {
            _world.Dispose();
            _world = null;

            _worldScreen.IsFocused = false;
            _worldScreen.Dispose();
            _worldScreen = null;

            GC.SuppressFinalize(this);
        }
    }
}
