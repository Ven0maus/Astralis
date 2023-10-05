using Astralis.Extended.SadConsole;
using Astralis.Scenes.GameplayScenes;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Linq;

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
        private OverworldScene _overworldScene;

        private const float _fontSize = 1.5f;

        public MainMenuScreen(OverworldScene overworldScene) : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            FontSize = new Point((int)(Font.GlyphWidth * _fontSize), (int)(Font.GlyphHeight * _fontSize));

            _overworldScene = overworldScene;
            _optionsScreen = new OptionsScreen() { IsVisible = false }; Children.Add(_optionsScreen);
            _loadGameScreen = new LoadGameScreen() { IsVisible = false }; Children.Add(_loadGameScreen);

            Surface.DefaultBackground = Color.Transparent;
            Surface.Fill(background: Color.Transparent);
        }

        ~MainMenuScreen()
        {
            Dispose();
        }

        public void Initialize()
        {
            CreateMenuTitle();
            CreateMenuButtons();
        }

        private void CreateMenuTitle()
        {

        }

        private void CreateMenuButtons()
        {
            var buttons = (ButtonType[])Enum.GetValues(typeof(ButtonType));
            var maxButtonWidth = (int)(buttons.Max(a => a.ToString().Length) * _fontSize);

            const int buttonHeight = 3;
            int centerX = (int)(Width / _fontSize) / 2;
            int centerY = (int)(Height / _fontSize) / 2;
            int buttonStartX = centerX - (maxButtonWidth / 2);
            int buttonStartY = centerY - (((int)(buttons.Length * _fontSize) / 2) * (int)(buttonHeight / _fontSize));

            int currentY = buttonStartY;
            for (int i = 0; i < buttons.Length; i++)
            {
                var value = buttons[i];
                var button = new ButtonBox(20, 3)
                {
                    Position = new Point(buttonStartX, currentY),
                    Text = value.ToString().Replace("_", " "),
                    Name = value.ToString()
                };
                button.Click += Button_Click;
                SetButtonTheme(button);
                Controls.Add(button);

                currentY += button.Height;
            }
        }

        private Colors _buttonTheme;
        private void SetButtonTheme(ButtonBox button)
        {
            if (_buttonTheme == null)
            {
                _buttonTheme = button.FindThemeColors().Clone();
                _buttonTheme.Lines.SetColor(Color.Black);
                _buttonTheme.SetForeground(Color.Black);
                _buttonTheme.SetBackground(Color.Transparent);
                _buttonTheme.RebuildAppearances();
            }
            button.SetThemeColors(_buttonTheme);
        }

        private bool _buttonClicked = false;
        private void Button_Click(object sender, EventArgs e)
        {
            if (_buttonClicked) return;
            _buttonClicked = true;

            var button = (ButtonBox)sender;
            var type = Enum.Parse<ButtonType>(button.Name);

            switch (type)
            {
                case ButtonType.New_Game:
                    NewGame();
                    break;
                case ButtonType.Load_Game:
                    LoadGame();
                    break;
                case ButtonType.Options:
                    Options();
                    break;
                case ButtonType.Exit_Game:
                    ExitGame();
                    break;
            }
        }

        private void InitNewGame(object sender, EventArgs args)
        {
            _overworldScene.MainMenuCallBack -= InitNewGame;
            _overworldScene = null;

            Game.Instance.Screen = new OverworldScene();
            ((OverworldScene)Game.Instance.Screen).StartGame();
        }

        private void NewGame()
        {
            _overworldScene.MainMenuCallBack += InitNewGame;
            _overworldScene.FadeOutMainMenu();
        }

        private void LoadGame()
        {
            // TODO
        }

        private void Options()
        {
            // TODO
        }

        private static void ExitGame()
        {
            Environment.Exit(0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_optionsScreen != null)
            {
                _optionsScreen.IsFocused = false;
                _optionsScreen?.Dispose();
                _optionsScreen = null;
            }

            if (_loadGameScreen != null)
            {
                _loadGameScreen.IsFocused = false;
                _loadGameScreen.Dispose();
                _loadGameScreen = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
