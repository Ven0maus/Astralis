using Astralis.Extended;
using Astralis.Extended.Effects;
using Astralis.Extended.SadConsoleExt;
using Astralis.Extended.SadConsoleExt.Controls;
using Astralis.GameCode.Npcs;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly MainMenuScreen _mainMenuScreen;
        private Phase _currentPhase = Phase.Design;

        // PHASE DESIGN
        private ScreenSurface _characterBorderScreen, _characterView;
        private TextBox _name;
        private ComboBox _gender, _race, _class;
        private ScColorBar _skinColor, _hairColor, _shirtColor, _pantsColor;
        private Facing _characterFacing = Facing.Forward;

        // PHASE TRAIT SELECTION
        private ListBox _availableTraits, _selectedTraits;

        // Original values
        private Color _origSkinColor, _origHairColor, _origShirtColor, _origPantsColor;

        public CharacterCreationScreen(MainMenuScreen mainMenuScreen, Action<object, WorldScreen> startGameMethod) : 
            base((int)(Constants.ScreenWidth / 100f * 35), 
                (int)(Constants.ScreenHeight / 100f * 50))
        {
            _mainMenuScreen = mainMenuScreen;
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
                Font = Game.Instance.Fonts[Constants.Fonts.NpcFonts.BaseNpc],
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

            // Make views visible again
            _currentPhase = Phase.Design;
            _characterBorderScreen.IsVisible = true;
            _characterView.IsVisible = true;
            Controls.Where(control => control.Name == null || control.Name.Equals(Phase.Design.ToString()) || control.Name == "Continue" || control.Name == "Cancel")
                .ForEach(a => a.IsVisible = true);
        }

        public ScreenSurface[] GetSurfaces()
        {
            return new[] { this, _characterBorderScreen, _characterView };
        }

        private void Initialize()
        {
            InitScreenVisual(this);
            CreateTitle();
            CreateControlsDesignPhase();
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

        private void CreateControlsDesignPhase()
        {
            var currentPosition = new Point((int)(Width / 100f * 9), _characterBorderScreen.Position.Y -2);
            _name = AddTextBox("Name:", currentPosition, Phase.Design);

            var genders = Enum.GetValues<Gender>();
            _gender = AddComboBox("Gender:", currentPosition += new Point(0, 3), genders.Cast<object>().ToArray(), Phase.Design);
            _gender.SelectedItemChanged += DrawCharacter;

            var races = Enum.GetValues<Race>().OrderBy(a => a);
            _race = AddComboBox("Race:", currentPosition += new Point(0, 3), races.Cast<object>().ToArray(), Phase.Design);
            _race.SelectedItemChanged += ChangeRace;

            var classes = Enum.GetValues<Class>().OrderBy(a => a);
            _class = AddComboBox("Class:", currentPosition += new Point(0, 3), classes.Cast<object>().ToArray(), Phase.Design);

            var skinColors = Constants.Fonts.NpcFonts.GetSkinColors((Race)_race.SelectedItem);
            _skinColor = AddColorBar("Skin color:", currentPosition += new Point(0, 3), skinColors[0], skinColors[1], Phase.Design);
            _origSkinColor = _skinColor.SelectedColor;
            _skinColor.ColorChanged += DrawCharacter;
            _hairColor = AddColorBar("Hair color:", currentPosition += new Point(0, 3), Phase.Design);
            _origHairColor = _hairColor.SelectedColor;
            _hairColor.ColorChanged += DrawCharacter;
            _shirtColor = AddColorBar("Shirt color:", currentPosition += new Point(0, 3), Phase.Design);
            _origShirtColor = _shirtColor.SelectedColor;
            _shirtColor.ColorChanged += DrawCharacter;
            _pantsColor = AddColorBar("Pants color:", currentPosition += new Point(0, 3), Phase.Design);
            _origPantsColor = _pantsColor.SelectedColor;
            _pantsColor.ColorChanged += DrawCharacter;

            var randomizeButton = new ButtonBox(_characterBorderScreen.Width - 3, 3)
            {
                Text = "Randomize",
                Name = Phase.Design.ToString(),
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y - 3)
            };
            randomizeButton.Click += ClickRandomize;
            SetControlTheme(randomizeButton);
            Controls.Add(randomizeButton);

            var rotateButton = new ButtonBox(3, 3)
            {
                Text = ((char)15).ToString(),
                Name = Phase.Design.ToString(),
                Position = new Point(_characterBorderScreen.Position.X + randomizeButton.Width, _characterBorderScreen.Position.Y - 3)
            };
            rotateButton.Click += RotateCharacter;
            SetControlTheme(rotateButton);
            Controls.Add(rotateButton);

            var continueButton = new ButtonBox(_characterBorderScreen.Width, 3)
            {
                Text = "Continue",
                Name = "Continue",
                Position = new Point(_characterBorderScreen.Position.X, _characterBorderScreen.Position.Y + _characterBorderScreen.Width)
            };
            continueButton.Click += ClickContinue;
            SetControlTheme(continueButton);
            Controls.Add(continueButton);

            var cancelButton = new ButtonBox(8, 3)
            {
                Text = "Cancel",
                Name = "Cancel",
                Position = new Point(Width - 10, 2)
            };
            cancelButton.Click += ClickCancel;
            SetControlTheme(cancelButton);
            Controls.Add(cancelButton);
        }

        private void CreateControlsTraitSelectionPhase()
        {
            _selectedTraits = AddListBox("Selected:", new Point(6, 8), new[] { "Test3" }, Phase.TraitSelection);
            _selectedTraits.Surface.DefaultBackground = Color.Black;
            _availableTraits = AddListBox("Traits:", _selectedTraits.Position + new Point(_selectedTraits.Width + 3, 0), new[] { "Test", "Test2" }, Phase.TraitSelection);
            _availableTraits.Surface.DefaultBackground = Color.Black;
        }

        private void ClickCancel(object sender, EventArgs e)
        {
            // Reset all fields to original
            _name.Text = "";
            _name.CaretPosition = 0;
            _race.SelectedIndex = 0;
            _gender.SelectedIndex = 0;
            _class.SelectedIndex = 0;
            _skinColor.SelectedColor = _origSkinColor;
            _hairColor.SelectedColor = _origHairColor;
            _shirtColor.SelectedColor = _origShirtColor;
            _pantsColor.SelectedColor = _origPantsColor;
            _characterFacing = Facing.Forward;

            DrawCharacter(null, null);

            foreach (var control in Controls)
            {
                control.IsVisible = false;
                control.IsDirty = true;
            }

            _mainMenuScreen.TransitionFromCharacterCreationScreen();
        }

        private void DrawCharacter(object sender, EventArgs args)
        {
            _ = NpcFontHelper.CreateNpcGlyph(_characterFacing,
                (Gender)_gender.SelectedItem,
                _skinColor.SelectedColor,
                _hairColor.SelectedColor,
                _shirtColor.SelectedColor,
                _pantsColor.SelectedColor,
                false,
                _characterView);

            _characterView.IsDirty = true;
        }

        private void ChangeRace(object sender, ListBox.SelectedItemEventArgs e)
        {
            var skinColors = Constants.Fonts.NpcFonts.GetSkinColors((Race)e.Item);
            _skinColor.StartingColor = skinColors[0];
            _skinColor.EndingColor = skinColors[1];

            DrawCharacter(sender, e);
        }

        private void ClickContinue(object sender, EventArgs e)
        {
            switch (_currentPhase)
            {
                case Phase.Design:
                    if (!ValidateDesignPhase())
                        return;
                    TransitionTo(Phase.TraitSelection);
                    break;
                case Phase.TraitSelection:
                    // Adds the glyph and its decorators to the font
                    AddCharacterToNpcFont();

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

        private void AddCharacterToNpcFont()
        {
            SadFont sadFont = NpcFontHelper.GetProceduralNpcFont();

            var facings = new[] { Facing.Forward, Facing.Left, Facing.Backwards };
            foreach (var facing in facings)
            {
                _ = NpcFontHelper.CreateNpcGlyph(facing,
                    (Gender)_gender.SelectedItem,
                    _skinColor.SelectedColor,
                    _hairColor.SelectedColor,
                    _shirtColor.SelectedColor,
                    _pantsColor.SelectedColor,
                    true);
            }

            NpcFontHelper.SaveFont(sadFont);
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

            // Adjust screen components to new phase
            _characterView.IsVisible = false;
            _characterBorderScreen.IsVisible = false;

            // Hide current controls
            Controls.Where(control => control.Name.Equals(Phase.Design.ToString()))
                .ForEach(a => a.IsVisible = false);

            // Add new controls
            CreateControlsTraitSelectionPhase();
        }

        private bool ValidateDesignPhase()
        {
            var name = _name.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                var message = "Please fill in a name for your new character!";
                ScWindow.Message(message, " Ok ", message.Length);
                return false;
            }
            return true;
        }

        private ComboBox AddComboBox(string labelText, Point position, object[] values, Phase phase)
        {
            var label = new Label(labelText) { Name = phase.ToString(), Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            int dropdownSize = values.Length + 2;
            if (dropdownSize > 10)
                dropdownSize = 10;

            var comboBox = new ComboBox(15, 15, dropdownSize, values) { Name = phase.ToString(), Position = position };
            Controls.Add(comboBox);

            return comboBox;
        }

        private ListBox AddListBox(string labelText, Point position, object[] values, Phase phase)
        {
            var label = new Label(labelText) { Name = phase.ToString(), Position = position + Direction.Up + Direction.Right };
            SetLabelTheme(label);
            Controls.Add(label);

            var listBox = new ListBox(14, 8) { Name = phase.ToString(), Position = position };
            listBox.DrawBorder = true;
            SetControlTheme(listBox);
            foreach (var value in values)
                listBox.Items.Add(value);
            Controls.Add(listBox);

            return listBox;
        }

        private TextBox AddTextBox(string labelText, Point position, Phase phase)
        {
            var label = new Label(labelText) { Name = phase.ToString(), Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            var textbox = new TextBox(15) { Name = phase.ToString(), Position = position };
            Controls.Add(textbox);

            return textbox;
        }

        private ScColorBar AddColorBar(string labelText, Point position, Color start, Color end, Phase phase)
        {
            var label = new Label(labelText) { Name = phase.ToString(), Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            var colorBar = new ScColorBar(15)
            {
                Name = phase.ToString(),
                Position = position,
                StartingColor = start,
                EndingColor = end
            };

            Controls.Add(colorBar);

            return colorBar;
        }

        private ScColorBar AddColorBar(string labelText, Point position, Phase phase)
        {
            var label = new Label(labelText) { Name = phase.ToString(), Position = position + Direction.Up };
            SetLabelTheme(label);
            Controls.Add(label);

            // Array must be same width (15)
            var colorBar = new ScColorBar(15, Constants.Fonts.NpcFonts.PredefinedColors)
            {
                Name = phase.ToString(),
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
        private void SetControlTheme(ControlBase control)
        {
            if (_buttonTheme == null)
            {
                _buttonTheme = control.FindThemeColors().Clone();
                _buttonTheme.Lines.SetColor(Color.Green);
                _buttonTheme.ControlBackgroundNormal.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundSelected.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundMouseOver.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundMouseDown.SetColor(Color.Transparent);
                _buttonTheme.ControlBackgroundFocused.SetColor(Color.Transparent);
                _buttonTheme.RebuildAppearances();
            }
            control.SetThemeColors(_buttonTheme);
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
