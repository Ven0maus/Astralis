using Astralis.GameCode.Items;
using Astralis.GameCode.Npcs;
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
        private const int _slotsPerRow = 4;
        private const int _totalRows = 4;

        public static InventoryScreen Instance { get; private set; }
        private IInventory Inventory { get { return Player.Instance.Inventory; } }

        private readonly List<Slot> _inventorySlots;
        private readonly List<Slot> _equipmentSlots;

        public InventoryScreen() : base(34, 26)
        {
            Instance = this;
            Position = new Point(Constants.ScreenWidth / 2 - Width / 2, Constants.ScreenHeight / 2 - Height / 2);

            var borderColor = new ColoredGlyph(Color.Green, Color.Black);
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, borderColor));

            var title = "Equipment";
            Surface.Print(Width / 4 - title.Length / 2, 1, title, Color.LightGoldenrodYellow);

            var title2 = "Inventory";
            Surface.Print((Width / 2 + Width / 4) - title2.Length / 2, 1, title2, Color.LightGoldenrodYellow);

            _inventorySlots = new List<Slot>();
            InitSlotSurfaces(_slotsPerRow, _totalRows, fontSize: 48, originalFontSize: 16);

            _equipmentSlots = new List<Slot>();
            InitEquipmentSurfaces();

            Surface.DefaultBackground = Color.Transparent;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Surface[x, y].Background = Color.Lerp(Color.Black, Color.Transparent, 0.05f);

            IsVisible = false;
        }

        public void Show()
        {
            IsVisible = true;
            IsFocused = true;
        }

        public void Hide()
        {
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

        private void InitEquipmentSurfaces()
        {
            // TODO
        }

        private void InitSlotSurfaces(int totalX, int totalY, int fontSize, int originalFontSize)
        {
            var sizePerSlot = fontSize / originalFontSize;
            var sizeDiff = (int)Math.Floor((double)sizePerSlot / 2) + 1;
            var sizeMultiplier = originalFontSize * sizeDiff;

            int count = 0;
            for (int x = 0; x < totalX * 2; x += 2)
            {
                for (int y = 0; y < totalY * 2; y += 2)
                {
                    var slot = new Slot(fontSize, false, count)
                    {
                        Position = new Point(
                            (Width / 2 * originalFontSize) + x * sizeMultiplier + originalFontSize / 2,
                            (originalFontSize * 3) + y * sizeMultiplier),
                        UsePixelPositioning = true
                    };
                    _inventorySlots.Add(slot);
                    Children.Add(slot);
                    count++;
                }
            }

            // Add main bar, 8 slots at the bottom
            for (int i=0; i < 8; i++)
            {
                var slot = new Slot(fontSize, true, i)
                {
                    Position = new Point(
                        (originalFontSize + (originalFontSize / 2)) + 2 * i * sizeMultiplier,
                        (Height * originalFontSize) - ((int)(originalFontSize * 4.5f))),
                    UsePixelPositioning = true
                };
                _inventorySlots.Add(slot);
                Children.Add(slot);
            }
        }
    }

    internal class Slot : ScreenSurface
    {
        private Item _item;
        public Item Item { get { return _item; } set { _item = value; AdjustItemIcon(); } }

        public int SlotIndex { get; private set; }
        public bool MainSlot { get; private set; }

        public Slot(int fontSize, bool mainSlot, int slotIndex) : base(1, 1)
        {
            FontSize = (fontSize, fontSize);
            Surface.DefaultBackground = Color.Lerp(Color.Lerp(Color.Black, Color.Gray, 0.45f), Color.Transparent, 0.25f);
            MainSlot = mainSlot;
            SlotIndex = slotIndex;
        }

        private void AdjustItemIcon()
        {
            if (Item == null)
            {
                Surface.Clear();
            }
            else
            {
                // TODO

            }
        }
    }
}
