using Astralis.Scenes.GameplayScenes;
using Astralis.Scenes.MainMenuScene;
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
                    f.AddExtraFonts(Constants.Fonts.Aesomatica, Constants.Fonts.WorldObjects, Constants.Fonts.LordNightmare);
                });

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }

        private static IScreenObject DefineStartupScreen(Game game)
        {
            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager);
            Resolution.SetResolutionFromCurrentDisplayMonitor(Constants.FullScreen);

            if (!Constants.DebugMode)
                return new MainMenuScreen();
            else
                return new OverworldScene();
        }
    }
}