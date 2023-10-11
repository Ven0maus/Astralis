using Astralis;
using SadConsole;
using SadConsole.Configuration;

namespace NoiseGenerator
{
    internal class Program
    {
        private static void Main()
        {
            Settings.WindowTitle = "Noise Generator";
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            Settings.AllowWindowResize = true;

            Builder gameStartup = new Builder()
                .SetStartingScreen(DefineStartupScreen)
                .IsStartingScreenFocused(false)
                .ConfigureFonts((f, Game) =>
                {
                    f.AddExtraFonts(Constants.Fonts.UserInterfaceFonts.Anno);
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
            return new View(Resolution.ScreenCellsX, Resolution.ScreenCellsY);
        }
    }
}