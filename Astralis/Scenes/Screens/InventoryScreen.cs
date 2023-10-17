using Astralis.GameCode.Items;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;

namespace Astralis.Scenes.Screens
{
    internal class InventoryScreen : ControlsConsole
    {
        public static InventoryScreen Instance { get; private set; }
        private IInventory _inventory;

        public InventoryScreen() : base(42, 32)
        {
            Instance = this;
            Position = new Point(Constants.ScreenWidth / 2 - Width / 2, Constants.ScreenHeight / 2 - Height / 2);

            var borderColor = new ColoredGlyph(Color.Green, Color.Black);
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, borderColor));

            var title = "Inventory";
            Surface.Print(Width / 2 - title.Length / 2, 1, title, Color.LightGoldenrodYellow);

            IsVisible = false;
        }

        public void Show(IInventory inventory)
        {
            _inventory = inventory;
            IsVisible = true;
            IsFocused = true;
        }

        public void Hide()
        {
            _inventory = null;
            IsVisible = false;
            IsFocused = false;
            Parent.IsFocused = true;
        }

        public override bool ProcessKeyboard(Keyboard keyboard)
        {
            if (keyboard.IsKeyPressed(Keys.Escape) || keyboard.IsKeyPressed(Keys.I))
            {
                Hide();
                return true;
            }
            return base.ProcessKeyboard(keyboard);
        }
    }
}
