using SadConsole;
using SadRogue.Primitives;
using System;

namespace Astralis.Scenes.Screens
{
    internal class CharacterCreationScreen : ScreenSurface
    {
        private readonly Action<object, WorldScreen> _startGameMethod;

        private bool _characterDesignCompleted = false;
        private bool _characterSpecializationCompleted = false;

        public CharacterCreationScreen(Action<object, WorldScreen> startGameMethod) : base(Constants.ScreenWidth / 2, (Constants.ScreenHeight - (int)(Constants.ScreenHeight / 100f * 25)))
        {
            _startGameMethod = startGameMethod;
            Surface.DefaultBackground = Color.Lerp(Color.Black, Color.Transparent, 0.01f);
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThick, new ColoredGlyph(Color.Green, Color.Black)));
            Initialize();
        }

        public void ShowHide()
        {
            IsVisible = !IsVisible;
            IsFocused = IsVisible;
        }

        private void Initialize()
        {
            CreateTitle();
            CreateControls();
        }

        private void CreateTitle()
        {
            // TODO: Improve
            const string title = "Character Creation";
            Surface.Print(Width / 2 - title.Length / 2, 3, title);
        }

        private void CreateControls()
        {
            // TODO
        }
    }
}
