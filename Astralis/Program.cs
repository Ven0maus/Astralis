using Astralis.Scenes;
using SadConsole;

namespace Astralis
{
    internal class Program
    {
        private static void Main()
        {
            Settings.WindowTitle = Constants.GameTitle;
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            Settings.AllowWindowResize = true;

            Game.Configuration gameStartup = new Game.Configuration()
                .SetStartingScreen(DefineStartupScreen)
                .IsStartingScreenFocused(false)
                .ConfigureFonts(f =>
                {
                    f.AddExtraFonts(
                        Constants.Fonts.LCD,
                        Constants.Fonts.WorldObjects,
                        Constants.Fonts.Anno);
                });

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static IScreenObject DefineStartupScreen(Game game)
        {
            var defaultFont = Game.Instance.Fonts[Constants.Fonts.LCD];

            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager, defaultFont);
            Resolution.SetResolutionFromCurrentDisplayMonitor(Constants.FullScreen);

            if (!Constants.DebugMode)
                return new MainMenuScene();
            else
                return new OverworldScene();
        }
    }
}