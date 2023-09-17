using Astralis.Scenes.MainMenuScene;
using SadConsole;

namespace Astralis
{
    internal class Program
    {
        private static void Main()
        {
            Settings.WindowTitle = Constants.GameTitle;
            Settings.AllowWindowResize = true;
            Settings.ResizeMode = Settings.WindowResizeOptions.Stretch;

            Game.Configuration gameStartup = new Game.Configuration()
                .SetScreenSize(Constants.ScreenWidth, Constants.ScreenHeight)
                .OnStart(OnGameStart)
                .SetStartingScreen<MainMenuScreen>();

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
        
        private static void OnGameStart()
        {
            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager);
            Resolution.SetResolution(Constants.DefaultResolution.X, Constants.DefaultResolution.Y, Constants.FullScreen);
        }
    }
}