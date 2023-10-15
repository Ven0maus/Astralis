using Astralis.Extended.Effects;
using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.Npcs;
using Astralis.GameCode.Npcs.Managers;
using Astralis.GameCode.WorldGen;
using Astralis.Scenes.Screens;
using SadConsole;
using SadRogue.Primitives;
using System;

namespace Astralis.Scenes
{
    internal class GameplayScene : Scene
    {
        private readonly World _world;
        private WorldScreen _worldScreen;

        public static GameplayScene Instance { get; private set; }

        public Point WorldSourceFontSize { get { return _worldScreen.WorldSourceFontSize; } }
        public Point WorldFontSize { get { return _worldScreen.FontSize; } }

        /// <summary>
        /// Used to push information between the mainmenu if this overworld is used as a background visual for it.
        /// </summary>
        public event EventHandler<WorldScreen> OnFadeFinished;

        public Player Player { get; private set; }
        public World World { get { return _world; } }
        public ConcurrentEntityManager EntityManager { get; private set; }

        private readonly bool _isMainMenu;

        public GameplayScene(bool mainMenu, string savePath = null)
        {
            Instance = this;
            _isMainMenu = mainMenu;

            if (Constants.DebugMode)
            {
                // Create gamedata npc font when running through debugmode
                _ = NpcFontHelper.GetGamedataNpcFont();
            }

            // For main menu scene, we can just use the base npc font, as we don't need the player visual
            EntityManager = new ConcurrentEntityManager();
            EntityManager.EntityComponent.AlternativeFont = _isMainMenu ?
                Game.Instance.Fonts[Constants.Fonts.NpcFonts.NpcFont] :
                Game.Instance.Fonts[Constants.Fonts.NpcFonts.GamedataNpcFont];

            // Generate world
            var worldGenerator = new WorldGenerator(Constants.GameSeed, new Extended.NoiseHelper(Constants.GameSeed));
            var chunkSize = Constants.WorldGeneration.ChunkSize;
            _world = new World(Constants.ScreenWidth, Constants.ScreenHeight, chunkSize, chunkSize, worldGenerator)
            {
                RaiseOnlyOnCellTypeChange = false
            };

            // Create world renderer
            _worldScreen = new WorldScreen(_world, _isMainMenu);
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
            // Add into entity manager
            EntityManager.SpawnAt(Player.Position, Player);

            // Set to a valid world location based on current world location (0, 0)
            Player.AdjustWorldPositionToValidLocation();

            // Adjust world camera position on the same position of the player
            SetCameraPosition(Player.WorldPosition);
        }

        public void SetCameraPosition(Point worldPosition)
        {
            _worldScreen.SetCameraPosition(worldPosition);
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
