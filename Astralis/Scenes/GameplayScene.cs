using Astralis.Extended.Effects;
using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.Npcs;
using Astralis.GameCode.Npcs.Managers;
using Astralis.GameCode.WorldGen;
using Astralis.Scenes.Screens;
using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Astralis.Scenes
{
    internal class GameplayScene : Scene
    {
        private readonly World _world;
        private WorldScreen _worldScreen;

        public static GameplayScene Instance { get; private set; }

        /// <summary>
        /// Used to push information between the mainmenu if this overworld is used as a background visual for it.
        /// </summary>
        public event EventHandler<WorldScreen> OnFadeFinished;

        public Player Player { get; private set; }
        public World World { get { return _world; } }
        public ConcurrentEntityManager EntityManager { get; private set; }

        private readonly bool _isMainMenu;
        private readonly int[] _npcGlyphs;

        public GameplayScene(bool mainMenu, string savePath = null)
        {
            Instance = this;
            _isMainMenu = mainMenu;

            // Generates procedural npc glyphs that can be used
            _npcGlyphs = NpcFontHelper.GenerateRandomNpcGlyphs(savePath);

            EntityManager = new ConcurrentEntityManager();
            EntityManager.EntityComponent.AlternativeFont = Game.Instance.Fonts[Constants.Fonts.NpcFonts.ProceduralNpcsFont];

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
        }

        ~GameplayScene()
        {
            Dispose();
        }

        public void StartGame(Player player)
        {
            Player = player;
            GameWorldInit();

            if (!Constants.DebugMode)
            {
                var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(2), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _worldScreen.GetSurfaces());
                fadeEffect.OnFinished = GameStart;
                Effects.Add(fadeEffect);
            }
            else
            {
                GameStart();
            }
        }

        /// <summary>
        /// Called before the world is transitioned
        /// </summary>
        private void GameWorldInit()
        {
            // Set screen location
            Player.Position = new Point(GameplayScene.Instance.World.Width / 2, GameplayScene.Instance.World.Height / 2);

            // Add into entity manager
            EntityManager.SpawnAt(Player.Position, Player);

            // Set to a valid world location based on current world location (0, 0)
            Player.AdjustWorldPositionToValidLocation();

            // Adjust world camera position on the same position of the player
            _worldScreen.SetCameraPosition(Player.WorldPosition);
        }

        /// <summary>
        /// Called after the game world is transitioned
        /// </summary>
        private void GameStart()
        {
            // TODO:
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
