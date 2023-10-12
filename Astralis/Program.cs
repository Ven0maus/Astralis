using Astralis.Scenes;
using SadConsole;
using SadConsole.Configuration;
using System;

namespace Astralis
{
    internal class Program
    {
        private static void Main()
        {
            Settings.WindowTitle = Constants.GameTitle;
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            Settings.AllowWindowResize = true;

            Builder gameStartup = new Builder()
                .SetStartingScreen(DefineStartupScreen)
                .IsStartingScreenFocused(false)
                .ConfigureFonts((fontConfig, game) =>
                {
                    fontConfig.AddExtraFonts(
                        Constants.Fonts.WorldFonts.WorldObjects,
                        Constants.Fonts.UserInterfaceFonts.Anno,
                        Constants.Fonts.NpcFonts.BaseNpc);
                });

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static IScreenObject DefineStartupScreen(Game game)
        {
            var defaultFont = Game.Instance.Fonts[Constants.Fonts.UserInterfaceFonts.Anno];

            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager, defaultFont);
            Resolution.SetResolutionFromCurrentDisplayMonitor(Constants.FullScreen);

            Constants.GameSeed = Constants.DebugMode ? Constants.GameSeed : new Random().Next(-1000000, 1000000);
            Constants.Random = new Random(Constants.GameSeed);

            if (!Constants.DebugMode)
                return new MainMenuScene();
            else
                return new OverworldScene(false);
        }
    }
}