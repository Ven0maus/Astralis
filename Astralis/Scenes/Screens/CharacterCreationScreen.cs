using Astralis.Extended;
using Astralis.Extended.Effects;
using Astralis.Extended.SadConsole;
using Astralis.Extended.SadConsole.Controls;
using Astralis.GameCode.Npcs;
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
        private enum Phase
        {
            Design,
            TraitSelection
        }

        private readonly Action<object, WorldScreen> _startGameMethod;

        private ScreenSurface _characterBorderScreen, _characterView;
        private TextBox _name;
        private ComboBox _gender, _race, _class;
        private ScColorBar _skinColor, _hairColor, _shirtColor, _pantsColor;
        private Facing _characterFacing = Facing.Forward;
        private Phase _currentPhase = Phase.Design;

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
            // Setup the main decorator indexes
            _characterView.Surface[0].Decorators = CellDecoratorHelpers.Pool.Rent();
            _characterView.Surface[0].Decorators.AddRange(new CellDecorator[] { default, default, default });
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

        public ScreenSurface[] GetSurfaces()
        {
            return new[] { this, _characterBorderScreen, _characterView };
        }

        private void Initialize()
        {
            InitScreenVisual(this);
            CreateTitle();
            CreateControls();
            DrawCharacter(null, null);
        }

        private static void InitScreenVisual(ScreenSurface surface)
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

            var genders = Enum.GetValues<Gender>();
            _gender = AddComboBox("Gender:", currentPosition += new Point(0, 3), genders.Cast<object>().ToArray());
            _gender.SelectedItemChanged += DrawCharacter;

            var races = Enum.GetValues<Race>().OrderBy(a => a);
            _race = AddComboBox("Race:", currentPosition += new Point(0, 3), races.Cast<object>().ToArray());
            _race.SelectedItemChanged += ChangeRace;

            var classes = Enum.GetValues<Class>().OrderBy(a => a);
            _class = AddComboBox("Class:", currentPosition += new Point(0, 3), classes.Cast<object>().ToArray());

            var skinColors = GetSkinColors((Race)_race.SelectedItem);
            _skinColor = AddColorBar("Skin color:", currentPosition += new Point(0, 3), skinColors[0], skinColors[1]);
            _skinColor.ColorChanged += DrawCharacter;
            _hairColor = AddColorBar("Hair color:", currentPosition += new Point(0, 3));
            _hairColor.ColorChanged += DrawCharacter;
            _shirtColor = AddColorBar("Shirt color:", currentPosition += new Point(0, 3));
            _shirtColor.ColorChanged += DrawCharacter;
            _pantsColor = AddColorBar("Pants color:", currentPosition += new Point(0, 3));
            _pantsColor.ColorChanged += DrawCharacter;

            var randomizeButton = new ButtonBox(_characterBorderScreen.Width - 3, 3)
            {
                Text = "Randomize",
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y - 3)
            };
            randomizeButton.Click += ClickRandomize;
            SetButtonTheme(randomizeButton);
            Controls.Add(randomizeButton);

            var rotateButton = new ButtonBox(3, 3)
            {
                Text = ((char)15).ToString(),
                Position = new Point(_characterBorderScreen.Position.X + randomizeButton.Width, _characterBorderScreen.Position.Y - 3)
            };
            rotateButton.Click += RotateCharacter;
            SetButtonTheme(rotateButton);
            Controls.Add(rotateButton);

            var continueButton = new ButtonBox(_characterBorderScreen.Width, 3)
            {
                Text = "Continue",
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y + _characterBorderScreen.Width)
            };
            continueButton.Click += ClickContinue;
            SetButtonTheme(continueButton);
            Controls.Add(continueButton);
        }

        private void DrawCharacter(object sender, EventArgs args)
        {
            // Last is facing (direction)
            var npcConfig = Enum.GetValues<NpcConfiguration>()
                .GroupBy(a => a.ToString().Split('_').Last());

            var facing = _characterFacing.ToString();
            if (facing == Facing.Left.ToString() || facing == Facing.Right.ToString())
                facing = "Sideways";

            var npcGroupGenderConfig = npcConfig.First(a => a.Key == facing)
                .Where(a => a.ToString().Split('_')[0].Equals(_gender.SelectedItem.ToString()))
                .ToArray();

            // Second is type (hair, shirt, etc), unless there are only 2 values it is the main
            var main = npcGroupGenderConfig.First(a => a.ToString().Split('_').Length == 2);
            var hair = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Hair"));
            var shirt = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Shirt"));
            var pants = npcGroupGenderConfig.First(a => a.ToString().Split('_')[1].Equals("Pants"));

            var mirror = !facing.Equals("Sideways") ? Mirror.None :
                _characterFacing == Facing.Right ? Mirror.Horizontal : Mirror.None;

            _characterView.Surface[0].Glyph = (int)main;
            _characterView.Surface[0].Foreground = _skinColor.SelectedColor;
            _characterView.Surface[0].Mirror = mirror;

            // Adjust indexes with new decorators
            _characterView.Surface[0].Decorators[0] = new CellDecorator(_hairColor.SelectedColor, (int)hair, mirror);
            _characterView.Surface[0].Decorators[1] = new CellDecorator(_shirtColor.SelectedColor, (int)shirt, mirror);
            _characterView.Surface[0].Decorators[2] = new CellDecorator(_pantsColor.SelectedColor, (int)pants, mirror);

            _characterView.IsDirty = true;
        }

        private static Color[] GetSkinColors(Race race)
        {
            switch (race)
            {
                case Race.Orc:
                    return new[] { "#4D2600".HexToColor(), "#006600".HexToColor() };
                case Race.Human:
                case Race.Elf:
                case Race.Dwarf:
                    return new[] { "#e6bc98".HexToColor(), "#3b2219".HexToColor() };
                default:
                    throw new NotImplementedException($"Skin color for race '{race}' not implemented.");
            }
        }

        private void ChangeRace(object sender, ListBox.SelectedItemEventArgs e)
        {
            var skinColors = GetSkinColors((Race)e.Item);
            _skinColor.StartingColor = skinColors[0];
            _skinColor.EndingColor = skinColors[1];

            DrawCharacter(sender, e);
        }

        private void ClickContinue(object sender, EventArgs e)
        {
            switch (_currentPhase)
            {
                case Phase.Design:
                    ValidateDesignPhase();
                    TransitionTo(Phase.TraitSelection);
                    // TODO: Remove this when 2nd phase is implemented
                    ClickContinue(sender, e);
                    break;
                case Phase.TraitSelection:
                    foreach (var control in Controls)
                        control.IsVisible = false;
                    var effect = new FadeEffect(TimeSpan.FromSeconds(1), FadeEffect.FadeContext.Both, FadeEffect.FadeMode.FadeOut, false, GetSurfaces());
                    var currentScene = (Scene)Game.Instance.Screen;
                    currentScene.Effects.Add(effect);
                    _startGameMethod.Invoke(null, null);
                    break;
                default:
                    throw new NotImplementedException($"Phase '{_currentPhase}' is not implemented.");
            }
        }

        private void ClickRandomize(object sender, EventArgs e)
        {
            _hairColor.SetRandomColor();
            _shirtColor.SetRandomColor();
            _pantsColor.SetRandomColor();
            _skinColor.SetRandomColor();
        }

        private void RotateCharacter(object sender, EventArgs e)
        {
            var values = Enum.GetValues<Facing>();
            var value = (int)_characterFacing;
            if (value == values.Length - 1)
                value = 0;
            else
                value++;
            _characterFacing = (Facing)value;
            DrawCharacter(sender, e);
        }

        private void TransitionTo(Phase phase)
        {
            _currentPhase = phase;

            // TODO: Adjust screen components to new phase
        }

        private void ValidateDesignPhase()
        {
            var name = _name.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                var message = "Please fill in a name for your new character!";
                ScWindow.Message(message, " Ok ", message.Length);
                return;
            }
        }

        private ComboBox AddComboBox(string labelText, Point position, object[] values)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            int dropdownSize = values.Length + 2;
            if (dropdownSize > 10)
                dropdownSize = 10;

            var comboBox = new ComboBox(15, 15, dropdownSize, values) { Position = position };
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

        private ScColorBar AddColorBar(string labelText, Point position, Color start, Color end)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            var colorBar = new ScColorBar(15)
            {
                Position = position,
                StartingColor = start,
                EndingColor = end
            };

            Controls.Add(colorBar);

            return colorBar;
        }

        private ScColorBar AddColorBar(string labelText, Point position)
        {
            var label = new Label(labelText) { Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            // Array must be same width (15)
            Color[] colors = new Color[]
            {
                Color.Coral,
                Color.Green,
                Color.Blue,
                Color.DarkOrange,
                Color.OliveDrab,
                Color.AnsiYellowBright,
                Color.Cyan,
                Color.Magenta,
                Color.Brown,
                Color.Teal,
                Color.Gray,
                Color.Lime,
                Color.Thistle,
                Color.DarkRed,
                Color.Indigo
            };

            var colorBar = new ScColorBar(15, colors)
            {
                Position = position,
            };

            Controls.Add(colorBar);

            return colorBar;
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
