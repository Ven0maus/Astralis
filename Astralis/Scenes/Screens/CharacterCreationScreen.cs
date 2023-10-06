using Astralis.Extended.SadConsole;
using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;

namespace Astralis.Scenes.Screens
{
    internal class CharacterCreationScreen : ControlSurface
    {
        private readonly Action<object, WorldScreen> _startGameMethod;

        private bool _characterDesignCompleted = false;
        private bool _characterSpecializationCompleted = false;

        private ScreenSurface _characterView;
        private TextBox _nameInput;

        public CharacterCreationScreen(Action<object, WorldScreen> startGameMethod) : 
            base((int)(Constants.ScreenWidth / 100f * 40), 
                (int)(Constants.ScreenHeight / 100f * 50))
        {
            _characterView = new ScreenSurface(18, 18);
            _characterView.DrawBox(new Rectangle(0, 0, _characterView.Width, _characterView.Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Green, Color.Transparent)));
            _characterView.Position = new Point((int)(Width / 100f * 60), (int)(Height / 100f * 30));
            Children.Add(_characterView);

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
            InitScreenVisual();
            CreateTitle();
            CreateControls();
            DrawCharacter();
        }

        private void DrawCharacter()
        {
            // TODO: Draw the character based on the selected character options
        }

        private void InitScreenVisual()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                        Surface.SetBackground(x, y, Color.DarkGreen);
                    else
                        Surface.SetBackground(x, y, Color.Lerp(Color.Black, Color.Transparent, 0.1f));
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
            _nameInput = AddTextBox("Name:", new Point((int)(Width / 100f * 10), _characterView.Position.Y - 1));

            var continueButton = new ButtonBox(_characterView.Width, 3)
            {
                Text = "Continue",
                Position = new Point(_characterView.Position.X, _characterView.Position.Y + _characterView.Width + 1)
            };
            continueButton.Click += ClickContinue;
            SetButtonTheme(continueButton);
            Controls.Add(continueButton);
        }

        private void ClickContinue(object sender, EventArgs e)
        {
            var name = _nameInput.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                var message = "Please fill in a name for your new character!";
                ScWindow.Message(message, "Understood", message.Length);
                return;
            }
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
                _buttonTheme.RebuildAppearances();
            }
            button.SetThemeColors(_buttonTheme);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_characterView != null)
            {
                _characterView.IsFocused = false;
                _characterView.Dispose();
                _characterView = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
