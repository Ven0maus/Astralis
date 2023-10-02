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
        private readonly ControlSurface _surface;
        private readonly ControlHost _controls;
        private readonly OptionsScreen _optionsScreen;
        private readonly LoadGameScreen _loadGameScreen;

        private readonly OverworldScene _overworldScene;
        private ScreenSurface _mainLayer, _shadowLayer;
        private bool _buttonClicked = false;

        public MainMenuScreen()
        {
            _overworldScene = new OverworldScene();
            Children.Add(_overworldScene);
            _overworldScene.OnMainMenuVisualLoaded += DisplayGameTitle;

            _surface = new ControlSurface(Constants.ScreenWidth, Constants.ScreenHeight);
            _surface.Font = Game.Instance.Fonts[Constants.Fonts.LordNightmare];
            _controls = new ControlHost();
            _surface.SadComponents.Add(_controls);
            _surface.Surface.DefaultBackground = Color.Transparent;
            _surface.IsVisible = false;
            Children.Add(_surface);

            foreach (var cell in _surface.Surface)
                cell.Background = Color.Transparent;
            _surface.Surface.IsDirty = true;

            _optionsScreen = new OptionsScreen() { IsVisible = false }; Children.Add(_optionsScreen);
            _loadGameScreen = new LoadGameScreen() { IsVisible = false }; Children.Add(_loadGameScreen);

            _overworldScene.Initialize(true);
        }

        private enum ButtonType
        {
            New_Game,
            Load_Game,
            Options,
            Exit_Game
        }

        private void DisplayGameTitle(object sender, EventArgs args)
        {
            _overworldScene.OnMainMenuVisualLoaded -= DisplayGameTitle;
            _surface.IsVisible = true;

            var titleParts = Constants.GameTitleFancy.Split("\r\n");
            int maxWidth = titleParts.Max(a => a.Length);
            int centerX = Width / 2;
            int startY = (int)(Height / 100f * 15);

            void ConfigureLayer(ScreenSurface layer, int xOffset, Color bg)
            {
                layer.Font = Game.Instance.Fonts[Constants.Fonts.LordNightmare];
                layer.Surface.DefaultBackground = Color.Transparent;
                layer.Surface.DefaultForeground = bg;
                layer.Position = new Point(centerX + xOffset - maxWidth / 2, startY - xOffset);
                Children.Add(layer);
            }

            // Add shadow layer with some offset
            _shadowLayer = new ScreenSurface(maxWidth, titleParts.Length) { UsePixelPositioning = true };
            ConfigureLayer(_shadowLayer, 1, Color.Transparent);
            _shadowLayer.Position *= new Point(_surface.FontSize.X, _surface.FontSize.Y);
            _shadowLayer.Position -= new Point(3, -11);
            _shadowLayer.IsVisible = false;

            _mainLayer = new ScreenSurface(maxWidth, titleParts.Length);
            ConfigureLayer(_mainLayer, 0, Color.Transparent);
            _mainLayer.IsVisible = false;

            // Prepare scatter effect and backing surfaces
            var scatteredGlyphs = new List<PositionedGlyph>();
            for (int i = 0; i < titleParts.Length; i++)
            {
                int x = 0;
                foreach (var glyph in titleParts[i])
                {
                    var pos = new Point(centerX - maxWidth / 2 + x, startY + i);
                    scatteredGlyphs.Add(new PositionedGlyph(new ColoredGlyph(Constants.GameTitleColor, _surface.Surface.DefaultBackground, glyph), pos));
                    x++;
                }
                _mainLayer.Print(0, i, titleParts[i], Constants.GameTitleColor, Color.Transparent);
                _shadowLayer.Print(0, i, titleParts[i], Constants.GameTitleShadowColor, Color.Transparent);
            }

            // Execute scatter effect and fade effect
            var scatterEffect = new ScatterEffect(scatteredGlyphs, _surface, TimeSpan.FromMilliseconds(1000))
            {
                OnFinished = () =>
                {
                    _surface.Clear();
                    _mainLayer.IsVisible = true;
                    _shadowLayer.IsVisible = true;

                    var fadeEffect = new FadeEffect(TimeSpan.FromMilliseconds(700), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeIn, false, _shadowLayer)
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
                _controls.Add(button);

                var flyInEffect = FlyInEffect.Create(button, FlyInEffect.Direction.Bottom, TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(150 * i));
                if (i == buttons.Length - 1)
                {
                    flyInEffect.OnFinished = () =>
                    {
                        foreach (var control in _controls)
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
            themeColors.ControlBackgroundNormal.SetColor(Color.Transparent);
            themeColors.ControlBackgroundDisabled.SetColor(Color.Transparent);
            themeColors.ControlForegroundNormal.SetColor(Constants.GameTitleShadowColor);
            themeColors.ControlForegroundDisabled.SetColor(Constants.GameTitleShadowColor);
            themeColors.ControlForegroundFocused.SetColor(Constants.GameTitleShadowColor);
            themeColors.ControlForegroundMouseDown.SetColor(Constants.GameTitleShadowColor);
            themeColors.ControlForegroundSelected.SetColor(Constants.GameTitleShadowColor);
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
                    ShowScreen(_loadGameScreen);
                    _buttonClicked = false;
                    break;
                case ButtonType.Options:
                    ShowScreen(_optionsScreen);
                    _buttonClicked = false;
                    break;
                case ButtonType.Exit_Game:
                    Environment.Exit(0);
                    break;
            }
        }

        private void StartGame()
        {
            _overworldScene.OnMainMenuVisualLoaded += (a, b) =>
            {
                var overworld = new OverworldScene();
                Game.Instance.Screen = overworld;
                overworld.Initialize(false);
                _buttonClicked = false;
            };

            // Drop the buttons
            var controls = _controls.Reverse().ToArray();
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
                                var positionedGlyph = new PositionedGlyph(new ColoredGlyph(Constants.GameTitleColor, _surface.Surface.DefaultBackground, glyph), destination);
                                positionedGlyph.Init(new Point(destination.X, startY + (titleParts.Length - 1) - i));
                                l.Add(positionedGlyph);
                                x++;

                                // Print to the real console
                                _surface.Surface[destination.X, startY + (titleParts.Length - 1) - i].CopyAppearanceFrom(positionedGlyph.Glyph, false);
                            }
                            positionedGlyphs.Add(l);
                        }
                        _surface.IsDirty = true;

                        for (int i = 0; i < positionedGlyphs.Count; i++)
                        {
                            var l = positionedGlyphs[i];
                            var fallingTextEffect = new MovingTextEffect(l, _surface, TimeSpan.FromMilliseconds(550), TimeSpan.FromMilliseconds(75 * i));
                            if (i == positionedGlyphs.Count - 1)
                            {
                                fallingTextEffect.OnFinished = () =>
                                {
                                    _overworldScene.DeintializeMainMenuVisuals();
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
            var screens = new ControlSurface[] { _optionsScreen, _loadGameScreen };
            foreach (var s in screens)
                s.IsVisible = false;
            //_mainLayer.IsVisible = false;
            //_shadowLayer.IsVisible = false;
            screen.IsVisible = true;
        }
    }
}
