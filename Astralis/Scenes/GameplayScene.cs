using Astralis.Extended.Effects;
using Astralis.Extended.SadConsoleExt;
using Astralis.GameCode.Npcs;
using Astralis.GameCode.Npcs.Config;
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
        public WorldScreen Display { get; private set; }

        public static GameplayScene Instance { get; private set; }

        public Point WorldSourceFontSize { get { return Display.WorldSourceFontSize; } }
        public Point WorldFontSize { get { return Display.FontSize; } }

        /// <summary>
        /// Used to push information between the mainmenu if this overworld is used as a background visual for it.
        /// </summary>
        public event EventHandler<WorldScreen> OnFadeFinished;

        public Player Player { get; private set; }
        public World World { get { return _world; } }
        public ConcurrentEntityManager EntityManager { get; private set; }

        private readonly bool _isMainMenu;

        public GameplayScene(bool mainMenu)
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
            Display = new WorldScreen(_world, _isMainMenu);
            Display.SadComponents.Add(EntityManager.EntityComponent);
            Children.Add(Display);

            if (Constants.DebugMode)
            {
                // Add fake player
                NpcFontHelper.GenerateRandomNpcGlyphs(Constants.Fonts.NpcFonts.GamedataNpcFont, amount: 1);
                StartGame(new Player((0, 0), Gender.Male, Race.Human, Class.Warrior, null));
            }
        }

        ~GameplayScene()
        {
            Dispose();
        }

        public void StartGame(Player player)
        {
            Player = player;
            GameWorldPreload();

            if (!Constants.DebugMode)
            {
                var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(2), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, new[] { Display })
                {
                    OnFinished = GameWorldAfterLoad
                };
                Effects.Add(fadeEffect);
            }
            else
            {
                GameWorldAfterLoad();
            }
        }

        /// <summary>
        /// Called before the world is transitioned
        /// </summary>
        private void GameWorldPreload()
        {
            // Add into entity manager
            EntityManager.SpawnAt(Player.Position, Player);

            // Set to a valid world location based on current world location (0, 0)
            Player.AdjustWorldPositionToValidLocation();

            // Adjust world camera position on the same position of the player
            Display.SetCameraPosition(Player.WorldPosition);
        }

        /// <summary>
        /// Called after the game world is transitioned
        /// </summary>
        private void GameWorldAfterLoad()
        {
            // TODO:
        }

        public void FadeIn(int seconds, Action<WorldScreen> actionOnStart = null)
        {
            actionOnStart?.Invoke(Display);
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(seconds), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, new[] { Display })
            {
                OnFinished = () =>
                {
                    OnFadeFinished?.Invoke(null, Display);
                }
            };
            Effects.Add(fadeEffect);
        }

        public void FadeOut(int seconds, Action<WorldScreen> actionOnStart = null)
        {
            actionOnStart?.Invoke(Display);
            var fadeEffect = new FadeEffect(TimeSpan.FromSeconds(seconds), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeOut, false, new[] { Display })
            {
                OnFinished = () =>
                {
                    OnFadeFinished?.Invoke(null, Display);
                }
            };
            Effects.Add(fadeEffect);
        }

        public override void Dispose()
        {
            if (Display != null)
            {
                Display.IsFocused = false;
                Display.Dispose();
                Display = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
