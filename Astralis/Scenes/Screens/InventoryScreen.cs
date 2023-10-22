using Astralis.GameCode.Items;
using SadConsole;
using SadConsole.Input;
using SadConsole.UI;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Astralis.Scenes.Screens
{
    internal class InventoryScreen : ControlsConsole
    {
        public static InventoryScreen Instance { get; private set; }
        private const int _slotsPerRow = 5;
        private readonly List<Slot> _slots;
        private IInventory _inventory;

        public InventoryScreen() : base(42, 27)
        {
            Instance = this;
            Position = new Point(Constants.ScreenWidth / 2 - Width / 2, Constants.ScreenHeight / 2 - Height / 2);

            var borderColor = new ColoredGlyph(Color.Green, Color.Black);
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, borderColor));

            var title = "Equipment";
            Surface.Print(Width / 4 - title.Length / 2, 1, title, Color.LightGoldenrodYellow);

            var title2 = "Inventory";
            Surface.Print((Width / 2 + Width / 4) - title2.Length / 2, 1, title2, Color.LightGoldenrodYellow);

            _slots = new List<Slot>();
            InitSlotSurfaces(_slotsPerRow, _slotsPerRow, fontSize: 48, originalFontSize: 16);

            Surface.DefaultBackground = Color.Transparent;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Surface[x, y].Background = Color.Lerp(Color.Black, Color.Transparent, 0.05f);

            IsVisible = false;
        }

        public void Show(IInventory inventory)
        {
            _inventory = inventory;
            AdjustSlots();
            IsVisible = true;
            IsFocused = true;
        }

        public void Hide()
        {
            _inventory = null;
            foreach (var slot in _slots)
                slot.Item = null;
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

        /// <summary>
        /// Sets the items in the slots based on the inventory set
        /// </summary>
        /// <returns></returns>
        private void AdjustSlots()
        {
            int slots = 0;
            foreach (var item in _inventory.Items)
                _slots[slots].Item = item.Value;
        }

        private void InitSlotSurfaces(int totalX, int totalY, int fontSize, int originalFontSize)
        {
            var sizePerSlot = fontSize / originalFontSize;
            var sizeDiff = (int)Math.Floor((double)sizePerSlot / 2) + 1;
            var sizeMultiplier = originalFontSize * sizeDiff;
            for (int x = 0; x < totalX * 2; x += 2)
            {
                for (int y = 0; y < totalY * 2; y += 2)
                {
                    var slot = new Slot(fontSize)
                    {
                        Position = new Point(
                            (Width / 2 * originalFontSize) + x * sizeMultiplier,
                            (originalFontSize * 3) + y * sizeMultiplier),
                        UsePixelPositioning = true
                    };
                    _slots.Add(slot);
                    Children.Add(slot);
                }
            }
        }
    }

    internal class Slot : ScreenSurface
    {
        private Item _item;
        public Item Item { get { return _item; } set { _item = value; AdjustItemIcon(); } }

        public Slot(int fontSize) : base(1, 1)
        {
            FontSize = (fontSize, fontSize);
            Surface.DefaultBackground = Color.Lerp(Color.Red, Color.Transparent, 0.25f);
        }

        private void AdjustItemIcon()
        {
            // TODO
        }
    }
}
