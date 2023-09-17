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
            var graphicsDevice = SadConsole.Host.Global.GraphicsDeviceManager;
            Resolution.Init(ref graphicsDevice);
            //Resolution.SetVirtualResolution(Constants.VirtualResolution.X, Constants.VirtualResolution.Y);
            Resolution.SetResolution(Constants.DefaultResolution.X, Constants.DefaultResolution.Y, true);
            //Resolution.BeginDraw();
        }
    }
}