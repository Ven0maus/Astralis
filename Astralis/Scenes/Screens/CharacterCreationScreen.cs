﻿using Astralis.Extended.SadConsole;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Linq;

namespace Astralis.Scenes.Screens
{
    internal class CharacterCreationScreen : ControlSurface
    {
        private readonly Action<object, WorldScreen> _startGameMethod;

        private bool _characterDesignCompleted = false;
        private bool _characterSpecializationCompleted = false;

        private ScreenSurface _characterBorderScreen, _characterView;
        private TextBox _name;
        private ComboBox _gender, _race;

        // TODO: For skin color, make a custom color selector that you can provide a set of colors to
        private ScrollBar _skinColor, _hair, _shirt, _pants, _shoes;

        public CharacterCreationScreen(Action<object, WorldScreen> startGameMethod) : 
            base((int)(Constants.ScreenWidth / 100f * 35), 
                (int)(Constants.ScreenHeight / 100f * 50))
        {
            _characterBorderScreen = new ScreenSurface(18, 18);
            for (int x=0; x < _characterBorderScreen.Width; x++)
            {
                for (int y = 0; y < _characterBorderScreen.Height; y++)
                {
                    if (x == 0 || y == 0 || x == _characterBorderScreen.Width - 1 || y == _characterBorderScreen.Height - 1)
                        continue;
                    _characterBorderScreen.Surface[x, y].Background = Color.Lerp(Color.Black, Color.Gray, 0.1f);
                }
            }
            _characterBorderScreen.Surface.DrawBox(new Rectangle(0, 0, _characterBorderScreen.Width, _characterBorderScreen.Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Green, Color.Transparent)));

            _characterBorderScreen.Position = new Point((int)(Width / 100f * 54), (int)(Height / 100f * 33));
            Children.Add(_characterBorderScreen);

            // Use 1,1 screen with font 16x16
            // Then to render the 1,1 cell in 16x16 space
            // We do 16font * 16cells = 256
            _characterView = new ScreenSurface(1, 1)
            {
                Font = Game.Instance.Fonts[Constants.Fonts.NpcFonts.PlayerNpc],
                FontSize = new Point(256, 256),
                UsePixelPositioning = true,
                Position = new Point(16, 15)
            };
            _characterView.Surface.DefaultBackground = Color.Transparent;
            _characterView.Surface[0].Glyph = 1;
            _characterBorderScreen.Children.Add(_characterView);

            _startGameMethod = startGameMethod;

            Initialize();
        }

        ~CharacterCreationScreen()
        {
            Dispose();
        }

        public void ShowHide()
        {
            IsVisible = !IsVisible;
            IsFocused = IsVisible;
        }

        private void Initialize()
        {
            InitScreenVisual(this);
            CreateTitle();
            CreateControls();
            DrawCharacter();
        }

        private void DrawCharacter()
        {
            // TODO: Draw the character based on the selected character options
        }

        private void InitScreenVisual(ScreenSurface surface)
        {
            surface.Surface.DefaultBackground = Color.Lerp(Color.Black, Color.Transparent, 0.15f);
            for (int x = 0; x < surface.Width; x++)
            {
                for (int y = 0; y < surface.Height; y++)
                {
                    if (x == 0 || y == 0 || x == surface.Width - 1 || y == surface.Height - 1)
                        surface.Surface.SetBackground(x, y, Color.DarkGreen);
                    else
                        surface.Surface.SetBackground(x, y, surface.Surface.DefaultBackground);
                }
            }
        }

        private void CreateTitle()
        {
            const string title = "Character Creation";
            Surface.Print(Width / 2 - title.Length / 2, 3, title);
        }

        private void CreateControls()
        {
            var currentPosition = new Point((int)(Width / 100f * 9), _characterBorderScreen.Position.Y -2);
            _name = AddTextBox("Name:", currentPosition);

            var genders = Enum.GetValues<Constants.NpcData.Gender>();
            _gender = AddComboBox("Gender:", currentPosition += new Point(0, 3), genders.Select(a => a.ToString()).ToArray());
            _gender.SelectedItemChanged += ChangeGender;

            var races = Enum.GetValues<Constants.NpcData.Race>();
            _race = AddComboBox("Race:", currentPosition += new Point(0, 3), races.Select(a => a.ToString()).ToArray());
            _race.SelectedItemChanged += ChangeRace;

            _skinColor = AddScrollBar("Skin color:", currentPosition += new Point(0, 3));
            _hair = AddScrollBar("Hair:", currentPosition += new Point(0, 3));
            _shirt = AddScrollBar("Shirt:", currentPosition += new Point(0, 3));
            _pants = AddScrollBar("Pants:", currentPosition += new Point(0, 3));
            _shoes = AddScrollBar("Shoes:", currentPosition += new Point(0, 3));

            var randomizeButton = new ButtonBox(_characterBorderScreen.Width, 3)
            {
                Text = "Randomize",
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y - 3)
            };
            randomizeButton.Click += ClickRandomize;
            SetButtonTheme(randomizeButton);
            Controls.Add(randomizeButton);

            var continueButton = new ButtonBox(_characterBorderScreen.Width, 3)
            {
                Text = "Continue",
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y + _characterBorderScreen.Width)
            };
            continueButton.Click += ClickContinue;
            SetButtonTheme(continueButton);
            Controls.Add(continueButton);
        }

        private void ChangeRace(object sender, ListBox.SelectedItemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ChangeGender(object sender, ListBox.SelectedItemEventArgs e)
        {
            var item = ((string)e.Item);
            _characterView.Surface[0].Glyph = item.Equals("Male", StringComparison.OrdinalIgnoreCase) ? 1 : 4;
            _characterView.Surface.IsDirty = true;
        }

        private void ClickContinue(object sender, EventArgs e)
        {
            var name = _name.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                var message = "Please fill in a name for your new character!";
                ScWindow.Message(message, "Understood", message.Length);
                return;
            }

            _startGameMethod.Invoke(null, null);
        }

        private void ClickRandomize(object sender, EventArgs e)
        {

        }

        private ComboBox AddComboBox(string labelText, Point position, string[] values)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            int minSize = values.Length + 2;
            if (minSize > 6)
                minSize = 6;

            var comboBox = new ComboBox(15, 15, minSize, values) { Position = position };
            Controls.Add(comboBox);

            return comboBox;
        }

        private TextBox AddTextBox(string labelText, Point position)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            var textbox = new TextBox(15) { Position = position };
            Controls.Add(textbox);

            return textbox;
        }

        private ScrollBar AddScrollBar(string labelText, Point position)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            var scrollBar = new ScrollBar(Orientation.Horizontal, 15)
            {
                Position = position,
            };

            Controls.Add(scrollBar);

            return scrollBar;
        }

        private Colors _labelTheme;
        private void SetLabelTheme(Label label)
        {
            if (_labelTheme == null)
            {
                Color color = Color.DarkGoldenrod;
                _labelTheme = label.FindThemeColors().Clone();
                _labelTheme.ControlForegroundNormal.SetColor(color);
                _labelTheme.ControlForegroundSelected.SetColor(color);
                _labelTheme.ControlForegroundMouseOver.SetColor(color);
                _labelTheme.ControlForegroundFocused.SetColor(color);
                _labelTheme.ControlBackgroundNormal.SetColor(Color.Transparent);
                _labelTheme.ControlBackgroundSelected.SetColor(Color.Transparent);
                _labelTheme.ControlBackgroundMouseOver.SetColor(Color.Transparent);
                _labelTheme.ControlBackgroundFocused.SetColor(Color.Transparent);
                _labelTheme.RebuildAppearances();
            }
            label.SetThemeColors(_labelTheme);
        }

        private Colors _buttonTheme;
        private void SetButtonTheme(ButtonBox button)
        {
            if (_buttonTheme == null)
            {
                _buttonTheme = button.FindThemeColors().Clone();
                _buttonTheme.Lines.SetColor(Color.Green);
                _buttonTheme.ControlBackgroundNormal.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundSelected.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundMouseOver.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundMouseDown.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundFocused.SetColor(Color.Transparent);
                _buttonTheme.RebuildAppearances();
            }
            button.SetThemeColors(_buttonTheme);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_characterBorderScreen != null)
            {
                _characterBorderScreen.IsFocused = false;
                _characterBorderScreen.Dispose();
                _characterBorderScreen = null;
            }

            if (_characterView != null)
            {
                _characterView.IsFocused = false;
                _characterView.Dispose();
                _characterView = null;
            }
        }
    }
}
