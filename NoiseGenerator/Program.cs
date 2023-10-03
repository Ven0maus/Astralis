using Astralis;
using SadConsole;

namespace NoiseGenerator
{
    internal class Program
    {
        private static void Main()
        {
            Settings.WindowTitle = "Noise Generator";
            Settings.ResizeMode = Settings.WindowResizeOptions.None;
            Settings.AllowWindowResize = true;

            Game.Configuration gameStartup = new Game.Configuration()
                .SetStartingScreen(DefineStartupScreen)
                .IsStartingScreenFocused(false)
                .ConfigureFonts(f =>
                {
                    f.AddExtraFonts(Constants.Fonts.LCD);
                });

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static IScreenObject DefineStartupScreen(Game game)
        {
            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager);
            Resolution.SetResolutionFromCurrentDisplayMonitor(Constants.FullScreen);
            return new View(Resolution.ScreenCellsX, Resolution.ScreenCellsY);
        }
    }
}