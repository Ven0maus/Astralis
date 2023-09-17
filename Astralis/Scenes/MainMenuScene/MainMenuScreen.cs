using Astralis.Extended;
using Astralis.Extended.Effects;
using Microsoft.Xna.Framework.Graphics;
using SadConsole;
using SadConsole.Instructions;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Linq;

namespace Astralis.Scenes.MainMenuScene
{
    internal class MainMenuScreen : Scene
    {
        private readonly Extended.Lazy<OptionsScreen> _optionsScreen;
        private readonly Extended.Lazy<LoadGameScreen> _loadGameScreen;

        public MainMenuScreen()
        {
            _optionsScreen = new Extended.Lazy<OptionsScreen>(() => { var screen = new OptionsScreen(); Children.Add(screen); return screen; });
            _loadGameScreen = new Extended.Lazy<LoadGameScreen>(() => { var screen = new LoadGameScreen(); Children.Add(screen); return screen; });

            DisplayGameTitle();
            AddMenuButtons();
        }

        private enum ButtonType
        {
            New_Game,
            Load_Game,
            Options,
            Exit_Game
        }

        private void DisplayGameTitle()
        {
            // TODO:
            // Scatter letters all over the screen
            // Make them move into position to form the title
            // Fade the shadow in after text is formed
            // Show the buttons after this transition is done

            var titleParts = Constants.GameTitleFancy.Split("\r\n");
            int maxWidth = titleParts.Max(a => a.Length);
            int centerX = Width / 2;
            int startY = (int)(Height / 100f * 15);

            void ConfigureLayer(ScreenSurface layer, int xOffset)
            {
                layer.Surface.DefaultBackground = Color.Transparent;
                layer.Surface.DefaultForeground = Color.Transparent;
                layer.Position = new Point(centerX + xOffset - maxWidth / 2, startY - xOffset);
                Children.Add(layer);
            }

            // Add shadow layer with some offset
            var shadowLayer = new ScreenSurface(maxWidth, titleParts.Length) { UsePixelPositioning = true };
            ConfigureLayer(shadowLayer, 1);
            shadowLayer.Position *= new Point(Constants.FontSize.X, Constants.FontSize.Y);
            shadowLayer.Position -= new Point(3, -11);

            var mainLayer = new ScreenSurface(maxWidth, titleParts.Length);
            ConfigureLayer(mainLayer, 0);

            for (int i = 0; i < titleParts.Length; i++)
            {
                mainLayer.Print(0, i, titleParts[i], Constants.GameTitleColor);
                shadowLayer.Print(0, i, titleParts[i], Constants.GameTitleShadowColor);
            }
        }

        private void AddMenuButtons()
        {
            int centerX = Width / 2;
            int centerY = Height / 2;

            ButtonType[] buttons = { ButtonType.New_Game, ButtonType.Load_Game, ButtonType.Options, ButtonType.Exit_Game };

            int largestLabelLength = buttons.Max(a => a.ToString().Length);
            int buttonWidth = largestLabelLength + 6;
            int buttonHeight = 3;

            Colors themeColors = null;
            for (int i = 0; i < buttons.Length; i++)
            {
                var buttonType = buttons[i];
                ButtonBox button = new(buttonWidth, buttonHeight)
                {
                    Position = new Point(centerX - buttonWidth / 2, centerY - buttonHeight * 2 + i * buttonHeight),
                    Text = buttonType.ToString().Replace('_', ' ')
                };

                themeColors ??= SetButtonBoxColorTheme(button);

                button.SetThemeColors(themeColors);
                button.Click += (sender, args) => HandleButtonClick(buttonType);
                Controls.Add(button);

                Effects.Add(FlyInEffect.Create(button, FlyInEffect.Direction.Bottom, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(150 * i)));
            }
        }

        private static Colors SetButtonBoxColorTheme(ButtonBox button)
        {
            var themeColors = button.FindThemeColors().Clone();
            themeColors.Lines.SetColor(Constants.GameTitleShadowColor);
            themeColors.ControlForegroundNormal.SetColor(Constants.GameTitleColor);
            themeColors.RebuildAppearances();
            return themeColors;
        }

        private void HandleButtonClick(ButtonType buttonType)
        {
            // TODO:
            // Do transition animation where the letters fall to the bottom of the screen
            // Starting from the bottom part of the text, all the way to the top part of the text
            // All the letters will lay on the bottom of the screen, added on top of eachother (decorators?)

            switch (buttonType)
            {
                case ButtonType.New_Game:
                    // TODO: Start game intro
                    break;
                case ButtonType.Load_Game:
                    ShowScreen(_loadGameScreen.Value);
                    break;
                case ButtonType.Options:
                    ShowScreen(_optionsScreen.Value);
                    break;
                case ButtonType.Exit_Game:
                    Environment.Exit(0);
                    break;
            }
        }

        private void ShowScreen(ControlSurface screen)
        {
            var screens = new ILazy[] {_optionsScreen, _loadGameScreen}
                .Where(a => a.IsValueCreated)
                .Select(a => a.Value)
                .Cast<ControlSurface>();

            foreach (var s in screens)
                s.IsVisible = false;

            screen.IsVisible = true;
        }
    }
}
