﻿using Astralis.Extended.Effects;
using Astralis.GameCode.Npcs;
using Astralis.GameCode.Npcs.Managers;
using Astralis.GameCode.WorldGen;
using Astralis.Scenes.Screens;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Astralis.Scenes
{
    internal class OverworldScene : Scene
    {
        private World _world;
        private WorldScreen _worldScreen;

        /// <summary>
        /// Used to push information between the mainmenu if this overworld is used as a background visual for it.
        /// </summary>
        public event EventHandler<WorldScreen> OnFadeFinished;

        public World World { get { return _world; } }
        public ConcurrentEntityManager EntityManager { get; private set; }

        public OverworldScene()
        {
            EntityManager = new ConcurrentEntityManager();
            EntityManager.GenerateProceduralNpcsFont();

            // Generate world
            var worldGenerator = new WorldGenerator(Constants.GameSeed, new Extended.NoiseHelper(Constants.GameSeed));
            var chunkSize = Constants.WorldGeneration.ChunkSize;
            _world = new World(Constants.ScreenWidth, Constants.ScreenHeight, chunkSize, chunkSize, worldGenerator)
            {
                RaiseOnlyOnCellTypeChange = false
            };

            // Create world renderer
            _worldScreen = new WorldScreen(_world);
            _worldScreen.SadComponents.Add(EntityManager.EntityComponent);
            Children.Add(_worldScreen);

            // TODO: Remove later after testing
            if (Game.Instance.Fonts.ContainsKey(Constants.Fonts.NpcFonts.ProceduralNpcsFont))
                EntityManager.SpawnAt(new Point(_world.Width / 4, _world.Height / 4), new Actor(new ColoredGlyph(Color.White, Color.Transparent, 2), 1));
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
            // TODO
        }

        public void FadeIn(int seconds, Action<WorldScreen> actionOnStart = null)
        {
            actionOnStart?.Invoke(_worldScreen);
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(seconds), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _worldScreen.GetSurfaces())
            {
                OnFinished = () =>
                {
                    OnFadeFinished?.Invoke(null, _worldScreen);
                }
            };
            Effects.Add(fadeEffect);
        }

        public void FadeOut(int seconds, Action<WorldScreen> actionOnStart = null)
        {
            actionOnStart?.Invoke(_worldScreen);
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(seconds), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeOut, false, _worldScreen.GetSurfaces())
            {
                OnFinished = () =>
                {
                    OnFadeFinished?.Invoke(null, _worldScreen);
                }
            };
            Effects.Add(fadeEffect);
        }

        public override void Dispose()
        {
            if (_worldScreen != null)
            {
                _worldScreen.IsFocused = false;
                _worldScreen.Dispose();
                _worldScreen = null;
            }
        }
    }
}
