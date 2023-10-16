using Astralis.Configuration;
using Astralis.Configuration.Models;
using Astralis.Extended;
using Astralis.Extended.Effects;
using Astralis.Extended.SadConsoleExt;
using Astralis.Extended.SadConsoleExt.Controls;
using Astralis.GameCode.Npcs;
using Astralis.GameCode.Npcs.Config;
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
        private ComboBox _traitPresets;

        // Original values
        private Color _origSkinColor, _origHairColor, _origShirtColor, _origPantsColor;

        private readonly NpcTraits NpcTraits = GameConfiguration.Load<NpcTraits>();

        private readonly HoverScreen _hoverScreen;

        public CharacterCreationScreen(MainMenuScreen mainMenuScreen, Action<object, WorldScreen> startGameMethod) :
            base((int)(Constants.ScreenWidth / 100f * 35),
                (int)(Constants.ScreenHeight / 100f * 50))
        {
            _mainMenuScreen = mainMenuScreen;
            _characterBorderScreen = new ScreenSurface(18, 18);
            for (int x = 0; x < _characterBorderScreen.Width; x++)
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
                Font = Game.Instance.Fonts[Constants.Fonts.NpcFonts.CharacterCreationBaseFont],
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

            _hoverScreen = new HoverScreen(30);
            _hoverScreen.Hide();
            Children.Add(_hoverScreen);

            Initialize();
        }

        ~CharacterCreationScreen()
        {
            Dispose();
        }

        public Player GetCreatedPlayer(Point worldPosition)
        {
            var traits = _selectedTraits.Items.Select(a => NpcTraits.Get.NpcTraits[((ColoredString)a).String]);
            return new Player(worldPosition, (Gender)_gender.SelectedItem, (Race)_race.SelectedItem, (Class)_class.SelectedItem, traits);
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
                .ForEach(a => a.IsVisible = IsVisible);
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
            var currentPosition = new Point((int)(Width / 100f * 9), _characterBorderScreen.Position.Y - 2);
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
            var traitPresetsList = NpcTraits.Get.NpcTraitPresets.Select(a => a.Key).ToList();
            traitPresetsList.Insert(0, string.Empty);
            var traits = NpcTraits.Get.NpcTraits
                .OrderByDescending(a => a.Value.Points)
                .Select(a => new ColoredString(a.Key, a.Value.Points >= 0 ? Color.Green : Color.Red, Color.Transparent))
                .ToArray();

            _selectedTraits = AddListBox("Selected:", new Point(3, 8), Array.Empty<object>(), Phase.TraitSelection);
            _selectedTraits.Surface.DefaultBackground = Color.Black;
            _selectedTraits.MouseMove += HandleHoverScreenTraits;
            _selectedTraits.MouseExit += HoverScreenExit;

            _availableTraits = AddListBox("Traits:", _selectedTraits.Position + new Point(_selectedTraits.Width + 3, 0), traits, Phase.TraitSelection);
            _availableTraits.Surface.DefaultBackground = Color.Black;
            _availableTraits.MouseMove += HandleHoverScreenTraits;
            _availableTraits.MouseExit += HoverScreenExit;

            _traitPresets = AddComboBox("Presets:", new Point(_selectedTraits.Position.X + 1, _selectedTraits.Position.Y + _selectedTraits.Height + 2), traitPresetsList.ToArray(), Phase.TraitSelection);
            _traitPresets.SelectedItemChanged += SelectPreset;

            UpdatePoints();
        }

        private void HandleHoverScreenTraits(object sender, ControlBase.ControlMouseState e)
        {
            if (!e.IsMouseOver)
            {
                _hoverScreen.Hide();
                return;
            }

            // Skip if we're on the border of the control
            var mousePos = e.MousePosition;
            if (mousePos.X == 0 || mousePos.Y == 0 || mousePos.X == e.Control.Width - 1 || mousePos.Y == e.Control.Height - 1)
            {
                _hoverScreen.Hide();
                return;
            }

            var listBox = (ListBox)e.Control;

            // Get the item the mouse is over, if there is any
            var index = mousePos.Y - 1;
            if (listBox.ScrollBar != null)
                index += listBox.ScrollBar.Value;

            // Hide when invalid position
            if (listBox.Items.Count == 0 || listBox.Items.Count <= index)
            {
                _hoverScreen.Hide();
                return;
            }

            var item = listBox.Items[index];
            var trait = NpcTraits.Get.NpcTraits[((ColoredString)item).String];

            var displayPosition = listBox.Position + mousePos;
            _hoverScreen.Show(displayPosition);

            // Set text
            var coloredString = ColoredString.Parser.Parse($"[c:r f:Yellow]Trait: [c:undo]{item}\n[c:r f:Orange]Points: [c:undo]{trait.Points}\n[c:r f:Cyan]Description: [c:undo]{trait.Description}");
            _hoverScreen.SetText(coloredString);

            // If display position goes outside of the viewport, push it back inside
            RepositionOffScreenHoverScreen();
        }

        protected void RepositionOffScreenHoverScreen()
        {
            // Calculate based on the fontsize
            IScreenSurface console = _mainMenuScreen;
            int screenBoundsX = console.Surface.Width;
            int screenBoundsY = console.Surface.Height;
            int containerBoundsX = (_hoverScreen.Position.X + _hoverScreen.Width) * console.FontSize.X / console.Font.GlyphWidth;
            int containerBoundsY = (_hoverScreen.Position.Y + _hoverScreen.Height) * console.FontSize.Y / console.Font.GlyphHeight;

            // We are going off the screen horizontally
            if (containerBoundsX >= screenBoundsX)
            {
                int diff = containerBoundsX - screenBoundsX;
                _hoverScreen.Position = new Point(_hoverScreen.Position.X - (int)Math.Round((decimal)diff / (console.FontSize.X / console.Font.GlyphWidth)), _hoverScreen.Position.Y);
            }

            // We are going off the screen vertically
            if (containerBoundsY >= screenBoundsY)
            {
                int diff = containerBoundsY - screenBoundsY;
                _hoverScreen.Position = new Point(_hoverScreen.Position.X, _hoverScreen.Position.Y - (int)Math.Round((decimal)diff / (console.FontSize.Y / console.Font.GlyphHeight)));
            }
        }

        private void HoverScreenExit(object sender, ControlBase.ControlMouseState e)
        {
            _hoverScreen.Hide();
        }

        private int _availablePoints = Constants.Configuration.NpcTraitsStartingPoints;

        private void RemoveSelectedTrait(object sender, ListBox.SelectedItemEventArgs args)
        {
            var obj = (ColoredString)args.Item;
            var req = GetRequiredPoints(obj.String);
            if (_availablePoints + req < 0)
            {
                ScWindow.Message("You cannot remove this trait as it will result in a negative available points balance.", "Ok");
                return;
            }

            _availablePoints += req;
            _selectedTraits.Items.Remove(args.Item);
            _availableTraits.Items.Add(args.Item);
            UpdatePoints();
            OrderAvailableTraits();
        }

        private void AddSelectedTrait(object sender, ListBox.SelectedItemEventArgs args)
        {
            var obj = (ColoredString)args.Item;
            var req = GetRequiredPoints(obj.String);
            if (_availablePoints < req)
            {
                ScWindow.Message("You do not have enough points available to select this trait.", "Ok");
                return;
            }

            _availablePoints -= req;
            _selectedTraits.Items.Add(args.Item);
            _availableTraits.Items.Remove(args.Item);
            UpdatePoints();
            OrderAvailableTraits();
        }

        private int GetRequiredPoints(string trait)
        {
            return NpcTraits.Get.NpcTraits[trait].Points;
        }

        private void OrderAvailableTraits()
        {
            var traits = _availableTraits.Items.OrderByDescending(a => NpcTraits.Get.NpcTraits[((ColoredString)a).String].Points).ToArray();
            foreach (var trait in traits)
                _availableTraits.Items.Remove(trait);
            foreach (var trait in traits)
                _availableTraits.Items.Add(trait);
        }

        private void UpdatePoints()
        {
            // Clear line on row 5
            Surface.Clear(1, 5, Width - 2);
            var text = $"Available points: {_availablePoints}";
            Surface.Print(Width / 2 - text.Length / 2, 5, text);
        }

        private void SelectPreset(object sender, ListBox.SelectedItemEventArgs e)
        {
            foreach (var trait in _selectedTraits.Items.OrderByDescending(a => NpcTraits.Get.NpcTraits[((ColoredString)a).String].Points).ToArray())
                RemoveSelectedTrait(sender, new ListBox.SelectedItemEventArgs(trait));

            if ((string)e.Item == string.Empty)
                return;

            var traits = _availableTraits.Items.Select(a => (ColoredString)a).ToHashSet();
            var preset = NpcTraits.Get.NpcTraitPresets[(string)e.Item];
            foreach (var value in preset.Traits.OrderBy(a => NpcTraits.Get.NpcTraits[a].Points))
            {
                var match = traits.First(a => a.String == value);
                AddSelectedTrait(this, new ListBox.SelectedItemEventArgs(match));
            }
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
            SadFont sadFont = NpcFontHelper.GetGamedataNpcFont(overwrite: true);

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

            if (_selectedTraits != null)
            {
                Controls.Where(control => control.Name.Equals(Phase.TraitSelection.ToString()))
                    .ForEach(a => a.IsVisible = true);
            }
            else
            {
                // Add new controls
                CreateControlsTraitSelectionPhase();
            }
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

            var isAllTraits = labelText == "Traits:";
            var listBox = new ListBox(isAllTraits ? 17 : 16, 19)
            {
                Name = phase.ToString(),
                Position = position,
                DrawBorder = true
            };
            listBox.SelectedItemExecuted += isAllTraits ? AddSelectedTrait : RemoveSelectedTrait;
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
