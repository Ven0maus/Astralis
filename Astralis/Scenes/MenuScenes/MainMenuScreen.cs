using Astralis.Extended;
using Astralis.Extended.Effects;
using Astralis.Extended.Effects.Core;
using Astralis.Extended.SadConsole;
using Astralis.Scenes.GameplayScenes;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Scenes.MainMenuScene
{
    internal class MainMenuScreen : Scene
    {
        private readonly Extended.Lazy<OptionsScreen> _optionsScreen;
        private readonly Extended.Lazy<LoadGameScreen> _loadGameScreen;

        private ScreenSurface _mainLayer, _shadowLayer;
        private bool _buttonClicked = false;

        public MainMenuScreen()
        {
            _optionsScreen = new Extended.Lazy<OptionsScreen>(() => { var screen = new OptionsScreen(); Children.Add(screen); return screen; });
            _loadGameScreen = new Extended.Lazy<LoadGameScreen>(() => { var screen = new LoadGameScreen(); Children.Add(screen); return screen; });

            DisplayGameTitle();
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
            _shadowLayer = new ScreenSurface(maxWidth, titleParts.Length) { UsePixelPositioning = true };
            ConfigureLayer(_shadowLayer, 1);
            _shadowLayer.Position *= new Point(Constants.FontSize.X, Constants.FontSize.Y);
            _shadowLayer.Position -= new Point(3, -11);
            _shadowLayer.IsVisible = false;

            _mainLayer = new ScreenSurface(maxWidth, titleParts.Length);
            ConfigureLayer(_mainLayer, 0);
            _mainLayer.IsVisible = false;

            // Prepare scatter effect and backing surfaces
            var scatteredGlyphs = new List<PositionedGlyph>();
            for (int i = 0; i < titleParts.Length; i++)
            {
                int x = 0;
                foreach (var glyph in titleParts[i])
                {
                    var pos = new Point(centerX - maxWidth / 2 + x, startY + i);
                    scatteredGlyphs.Add(new PositionedGlyph(new ColoredGlyph(Constants.GameTitleColor, Surface.Surface.DefaultBackground, glyph), pos));
                    x++;
                }
                _mainLayer.Print(0, i, titleParts[i], Constants.GameTitleColor);
                _shadowLayer.Print(0, i, titleParts[i], Constants.GameTitleShadowColor);
            }

            // Execute scatter effect and fade effect
            var scatterEffect = new ScatterEffect(scatteredGlyphs, Surface, TimeSpan.FromMilliseconds(1500))
            {
                OnFinished = () =>
                {
                    Surface.Clear();
                    _mainLayer.IsVisible = true;
                    _shadowLayer.IsVisible = true;

                    var fadeEffect = new FadeEffect(_shadowLayer, TimeSpan.FromMilliseconds(700), FadeEffect.FadeMode.FadeIn, false)
                    {
                        OnFinished = AddMenuButtons
                    };
                    Effects.Add(fadeEffect);
                }
            };
            Effects.Add(scatterEffect);
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
                button.IsEnabled = false;
                Controls.Add(button);

                var flyInEffect = FlyInEffect.Create(button, FlyInEffect.Direction.Bottom, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(150 * i));
                if (i == buttons.Length - 1)
                {
                    flyInEffect.OnFinished = () =>
                    {
                        foreach (var control in Controls)
                            control.IsEnabled = true;
                    };
                }
                Effects.Add(flyInEffect);
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
            if (_buttonClicked) return;
            _buttonClicked = true;

            switch (buttonType)
            {
                case ButtonType.New_Game:
                    StartGame();
                    break;
                case ButtonType.Load_Game:
                    ShowScreen(_loadGameScreen.Value);
                    _buttonClicked = false;
                    break;
                case ButtonType.Options:
                    ShowScreen(_optionsScreen.Value);
                    _buttonClicked = false;
                    break;
                case ButtonType.Exit_Game:
                    Environment.Exit(0);
                    break;
            }
        }

        private void StartGame()
        {
            // Drop the buttons
            var controls = Controls.Reverse().ToArray();
            for (int ci = 0; ci < controls.Length; ci++)
            {
                var control = controls[ci];
                var flyinEffect = new FlyInEffect(control.Position, new Point(control.Position.X, Height + control.Height), TimeSpan.FromMilliseconds(300), (pos) => { control.Position = pos; control.IsDirty = true; }, TimeSpan.FromMilliseconds(100 * ci));
                if (ci == 0)
                {
                    flyinEffect.OnFinished = () =>
                    {
                        // Drop the text and removing the backing surfaces
                        Children.Remove(_shadowLayer);
                        Children.Remove(_mainLayer);

                        // Prepare falling text effect
                        var titleParts = Constants.GameTitleFancy.Split("\r\n").Reverse().ToArray();
                        int maxWidth = titleParts.Max(a => a.Length);
                        int centerX = Width / 2;
                        int startY = (int)(Height / 100f * 15);
                        var positionedGlyphs = new List<List<PositionedGlyph>>();
                        for (int i = 0; i < titleParts.Length; i++)
                        {
                            var l = new List<PositionedGlyph>();
                            int x = 0;
                            foreach (var glyph in titleParts[i])
                            {
                                var destination = new Point(centerX - maxWidth / 2 + x, Height);
                                var positionedGlyph = new PositionedGlyph(new ColoredGlyph(Constants.GameTitleColor, Surface.Surface.DefaultBackground, glyph), destination);
                                positionedGlyph.Init(new Point(destination.X, startY + (titleParts.Length - 1) - i));
                                l.Add(positionedGlyph);
                                x++;

                                // Print to the real console
                                Surface.Surface[destination.X, startY + (titleParts.Length - 1) - i].CopyAppearanceFrom(positionedGlyph.Glyph, false);
                            }
                            positionedGlyphs.Add(l);
                        }
                        Surface.IsDirty = true;

                        for (int i = 0; i < positionedGlyphs.Count; i++)
                        {
                            var l = positionedGlyphs[i];
                            var fallingTextEffect = new MovingTextEffect(l, Surface, TimeSpan.FromMilliseconds(550), TimeSpan.FromMilliseconds(75 * i));
                            if (i == positionedGlyphs.Count - 1)
                            {
                                fallingTextEffect.OnFinished = () =>
                                {
                                    Game.Instance.Screen = new OverworldScene();
                                    _buttonClicked = false;
                                };
                            }
                            Effects.Add(fallingTextEffect);
                        }
                    };
                }
                Effects.Add(flyinEffect);
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
