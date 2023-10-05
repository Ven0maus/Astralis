using SadConsole.UI;
using System;

namespace Astralis.Scenes.Screens
{
    internal class MainMenuScreen : ControlsConsole
    {
        private enum ButtonType
        {
            New_Game,
            Load_Game,
            Options,
            Exit_Game
        }

        private OptionsScreen _optionsScreen;
        private LoadGameScreen _loadGameScreen;

        public MainMenuScreen() : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            FontSize = Font.GetFontSize(SadConsole.IFont.Sizes.Two);

            _optionsScreen = new OptionsScreen() { IsVisible = false }; Children.Add(_optionsScreen);
            _loadGameScreen = new LoadGameScreen() { IsVisible = false }; Children.Add(_loadGameScreen);
        }

        ~MainMenuScreen()
        {
            Dispose();
        }

        public void Initialize()
        {
            // TODO
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _optionsScreen.IsFocused = false;
            _optionsScreen.Dispose();
            _optionsScreen = null;

            _loadGameScreen.IsFocused = false;
            _loadGameScreen.Dispose();
            _loadGameScreen = null;

            GC.SuppressFinalize(this);
        }
    }
}
