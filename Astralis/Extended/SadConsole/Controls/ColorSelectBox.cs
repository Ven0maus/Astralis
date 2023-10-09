using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadConsole.UI.Windows;
using SadRogue.Primitives;
using System;

namespace Astralis.Extended.SadConsole.Controls
{
    /// <summary>
    /// A 1x1 box, when clicking shows a popup to select a color.
    /// Once the color is selected, the box then becomes this color.
    /// </summary>
    internal partial class ColorSelectBox : ControlBase
    {
        private IScreenSurface PreviousParent;
        public Color SelectedColor { get; private set; }

        private readonly ColorPickerPopup _colorSelector;

        public ColorSelectBox(Color defaultColor)
            : base(3, 1)
        {
            SelectedColor = defaultColor;
            _colorSelector = new ColorPickerPopup
            {
                CanDrag = false,
                Title = string.Empty,
                SelectedColor = defaultColor
            };
            _colorSelector.Closed += OnColorSelectorClosed;
            IsDirty = true;
        }

        private void OnColorSelectorClosed(object sender, EventArgs e)
        {
            var window = (ColorPickerPopup)sender;
            SelectedColor = window.SelectedColor;
            IsDirty = true;
        }

        protected override void OnParentChanged()
        {
            if (Parent != null && Parent is ControlHost ch)
            {
                PreviousParent?.Children.Remove(_colorSelector);

                var pc = ch.ParentConsole;
                pc.Children.Add(_colorSelector);
                PreviousParent = pc;
            }
        }

        protected override void OnPositionChanged()
        {
            base.OnPositionChanged();
            _colorSelector.Position = new Point(0, 0);
        }

        public void InvokeClick()
        {
            _colorSelector.Show(true);
        }

        internal void SetColor(Color color)
        {
            SelectedColor = color;
            IsDirty = true;
        }

        protected override void OnLeftMouseClicked(ControlMouseState state)
        {
            base.OnLeftMouseClicked(state);

            InvokeClick();
        }

        public override void UpdateAndRedraw(TimeSpan time)
        {
            if (!IsDirty) return;
            Surface[0].Glyph = '[';
            Surface[0].Foreground = Color.White;
            Surface[1].Background = SelectedColor;
            Surface[2].Glyph = ']';
            Surface[2].Foreground = Color.White;
        }
    }
}
