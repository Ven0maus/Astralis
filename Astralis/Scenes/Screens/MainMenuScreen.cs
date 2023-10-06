using Astralis.Extended;
using Astralis.Extended.SadConsole;
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

        private OverworldScene _backgroundOverworldScene;
        private OptionsScreen _optionsScreen;
        private LoadGameScreen _loadGameScreen;
        private CharacterCreationScreen _characterCreationScreen;

        public MainMenuScreen(OverworldScene overworldScene) : base(Constants.ScreenWidth, Constants.ScreenHeight)
        {
            Font = Game.Instance.Fonts[Constants.Fonts.Anno];

            _backgroundOverworldScene = overworldScene;
            _optionsScreen = new OptionsScreen() { IsVisible = false }; Children.Add(_optionsScreen);
            _loadGameScreen = new LoadGameScreen() { IsVisible = false }; Children.Add(_loadGameScreen);
            _characterCreationScreen = new CharacterCreationScreen(StartGame) { IsVisible = false }; Children.Add(_characterCreationScreen);

            int centerX = Width / 2;
            int centerY = Height / 2;
            _characterCreationScreen.Position = new Point(
                centerX - _characterCreationScreen.Width / 2, 
                centerY - _characterCreationScreen.Height / 2);

            Surface.DefaultBackground = Color.Lerp(Color.Black, Color.Transparent, 0.2f);
            Surface.Fill(background: Surface.DefaultBackground);
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
            var titleParts = Constants.GameTitleFancy.Split("\r\n");
            int maxLength = titleParts.Max(a => a.Length);
            int centerX = Width / 2;
            int startY = (int)(Height / 100f * 15);

            for (int i = 0; i < titleParts.Length; i++)
            {
                Surface.Print(centerX - (maxLength / 2), startY + i, titleParts[i], Constants.GameTitleColor, Surface.DefaultBackground);
            }
        }

        private void CreateMenuButtons()
        {
            var buttons = (ButtonType[])Enum.GetValues(typeof(ButtonType));

            const int buttonWidth = 20;
            const int buttonHeight = 3;

            int centerX = Width / 2;
            int centerY = Height / 2;
            int buttonStartX = centerX - (buttonWidth / 2);
            int buttonStartY = centerY - buttons.Length / 2 * buttonHeight;

            int currentY = buttonStartY;
            for (int i = 0; i < buttons.Length; i++)
            {
                var value = buttons[i];
                var button = new ButtonBox(buttonWidth, 3)
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
                _buttonTheme.Lines.SetColor(Constants.GameTitleShadowColor);
                _buttonTheme.SetForeground(Constants.GameTitleColor);
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

        private void NewGame()
        {
            IsFocused = false;

            // Hide buttons
            Controls.OfType<ButtonBox>().ForEach((a) => a.IsVisible = false);

            // Transition to character creation screen
            Surface.Clear();
            _characterCreationScreen.ShowHide();
        }

        private void StartGame(object sender, WorldScreen args)
        {
            if (args == null)
            {
                // Args is only null when manually called
                // Then trigger a fadeout, which on finished will 
                _backgroundOverworldScene.OnFadeFinished += StartGame;
                _backgroundOverworldScene.FadeOut(2, (ws) => ws.MainMenuCamera = false);
                return;
            }

            _backgroundOverworldScene.OnFadeFinished -= StartGame;

            Game.Instance.Screen = new OverworldScene();
            ((OverworldScene)Game.Instance.Screen).StartGame();
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

            if (_characterCreationScreen != null)
            {
                _characterCreationScreen.IsFocused = false;
                _characterCreationScreen.Dispose();
                _characterCreationScreen = null;
            }

            if (_backgroundOverworldScene != null)
            {
                _backgroundOverworldScene.IsFocused = false;
                _backgroundOverworldScene.Dispose();
                _backgroundOverworldScene = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
