using Astralis.Scenes.GameplayScenes;
using Astralis.Scenes.MainMenuScene;
using Microsoft.Xna.Framework.Graphics;
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
                .IsStartingScreenFocused(false)
                .ConfigureFonts(f =>
                {
                    f.AddExtraFonts(Constants.Fonts.Aesomatica);
                });

            if (!Constants.DebugMode)
                gameStartup = gameStartup
                    .SetStartingScreen<MainMenuScreen>();
            else
                gameStartup = gameStartup
                    .SetStartingScreen<OverworldScene>();

            Game.Create(gameStartup);
            Game.Instance.Run();
            Game.Instance.Dispose();
        }
        
        private static void OnGameStart()
        {
            Resolution.Init(SadConsole.Host.Global.GraphicsDeviceManager);
            int userDisplayWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int userDisplayHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            Resolution.SetResolution(userDisplayWidth, userDisplayHeight, Constants.FullScreen);
        }
    }
}