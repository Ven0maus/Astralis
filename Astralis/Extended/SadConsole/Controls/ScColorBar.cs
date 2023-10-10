using SadConsole;
using SadConsole.Input;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Linq;

namespace Astralis.Extended.SadConsole.Controls;

/// <summary>
/// A color bar control.
/// </summary>
public class ScColorBar : ControlBase
{
    /// <summary>
    /// Raised when the <see cref="SelectedColor"/> value changes.
    /// </summary>
    public event EventHandler ColorChanged;

    /// <summary>
    /// Internal use by theme.
    /// </summary>
    private Color[] _colorSteps;

    private Color _selectedColor;
    private Color _startingColor;
    private Color _endingColor;

    /// <summary>
    /// Gets or sets the color on the left-side of the bar.
    /// </summary>
    public Color StartingColor
    {
        get => _startingColor;
        set
        {
            _startingColor = value;
            _colorSteps = StartingColor.LerpSteps(EndingColor, Width);
            _selectedColor = _colorSteps[SelectedPosition];
            IsDirty = true;
        }
    }

    /// <summary>
    /// Gets or sets the color on the right-side of the bar.
    /// </summary>
    public Color EndingColor
    {
        get => _endingColor;
        set
        {
            _endingColor = value;
            _colorSteps = StartingColor.LerpSteps(EndingColor, Width);
            _selectedColor = _colorSteps[SelectedPosition];
            IsDirty = true;
        }
    }

    /// <summary>
    /// The selected color.
    /// </summary>
    public Color SelectedColor
    {
        get => _selectedColor;
        set
        {
            SetClosestIndex(value);

            if (_selectedColor != value)
            {
                _selectedColor = value;

                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private Color SelectedColorSafe
    {
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;

                ColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// The position on the bar currently selected.
    /// </summary>
    public int SelectedPosition { get; private set; }

    /// <summary>
    /// Creates a new color bar with the specified width.
    /// </summary>
    /// <param name="width">The width of the bar.</param>
    public ScColorBar(int width, Color[] predefinedColors = null) : base(width, 1)
    {
        StartingColor = Color.White;
        EndingColor = Color.Black;
        CanFocus = false;

        if (predefinedColors != null && predefinedColors.Length != width)
            throw new Exception("The predefined color array must be the same size as the control width.");

        _colorSteps = predefinedColors;
        if (_colorSteps != null)
            _selectedColor = _colorSteps[0];
    }

    public void SetRandomColor()
    {
        SelectedPosition = Constants.Random.Next(_colorSteps.Length);
        SelectedColorSafe = _colorSteps[SelectedPosition];
        IsDirty = true;
    }

    private void SetClosestIndex(Color color)
    {
        ColorMine.ColorSpaces.Rgb rgbColorStop = new ColorMine.ColorSpaces.Rgb() { R = color.R, G = color.G, B = color.B };
        Tuple<Color, double, int>[] colorWeights = new Tuple<Color, double, int>[Width];

        // Create a color weight for every cell compared to the color stop
        for (int x = 0; x < Width; x++)
        {
            ColorMine.ColorSpaces.Rgb rgbColor = new ColorMine.ColorSpaces.Rgb() { R = Surface[x, 0].Background.R, G = Surface[x, 0].Background.G, B = Surface[x, 0].Background.B };
            ColorMine.ColorSpaces.Cmy cmyColor = rgbColor.To<ColorMine.ColorSpaces.Cmy>();

            colorWeights[x] = new Tuple<Color, double, int>(Surface[x, 0].Background, rgbColorStop.Compare(cmyColor, new ColorMine.ColorSpaces.Comparisons.Cie1976Comparison()), x);
        }

        Tuple<Color, double, int> foundColor = colorWeights.OrderBy(t => t.Item2).First();
        SelectedPosition = foundColor.Item3;
        IsDirty = true;
    }

    /// <inheritdoc/>
    protected override void OnMouseIn(ControlMouseState info)
    {
        base.OnMouseIn(info);

        if (Parent.Host.CapturedControl == null)
        {
            if (info.OriginalMouseState.Mouse.LeftButtonDown)
            {
                Point location = info.MousePosition;

                SelectedPosition = location.X;
                SelectedColorSafe = Surface[SelectedPosition, 0].Background;
                IsDirty = true;

                Parent.Host.CaptureControl(this);
            }
        }
    }

    /// <inheritdoc/>
    public override bool ProcessMouse(MouseScreenObjectState info)
    {
        if (Parent.Host.CapturedControl == this)
        {
            if (info.Mouse.LeftButtonDown == false)
                Parent.Host.ReleaseControl();
            else
            {
                var newState = new ControlMouseState(this, info);
                Point location = newState.MousePosition;

                //if (info.ConsolePosition.X >= Position.X && info.ConsolePosition.X < Position.X + Width)
                if (location.X >= 0 && location.X <= Width - 1 && location.Y > -4 && location.Y < Height + 3)
                {
                    SelectedPosition = location.X;
                    SelectedColorSafe = Surface[SelectedPosition, 0].Background;
                }

                IsDirty = true;
            }
        }

        return base.ProcessMouse(info);
    }

    /// <inheritdoc/>
    public override void UpdateAndRedraw(TimeSpan time)
    {
        if (!IsDirty) return;

        Surface.Fill(Color.White, Color.Black, 0, null);

        for (int x = 0; x < Width; x++)
        {
            Surface[x, 0].Foreground = Color.Crimson;
            Surface[x, 0].Background = _colorSteps[x];
        }

        Surface[SelectedPosition, 0].Glyph = 4;
        IsDirty = false;
    }
}
