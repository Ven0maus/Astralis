using SadConsole;
using SadRogue.Primitives;

namespace Astralis.Extended.SadConsoleExt
{
    internal class HoverScreen : ScreenSurface
    {
        private readonly int _maxContentWidth;
        private readonly ScreenSurface _innerSurface;

        public HoverScreen(int width)
            : base(1, 1)
        {
            // -2 for border
            _innerSurface = new ScreenSurface(1, 1);
            _innerSurface.Position = new Point(1, 1);
            Children.Add(_innerSurface);

            _maxContentWidth = width - 2;
            Surface.DefaultBackground = Color.Black;

            UseMouse = false;
            UseKeyboard = false;
        }

        public void Show(Point position)
        {
            Position = position;
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void SetText(string text)
        {
            SetText(new ColoredString(text));
        }

        public void SetText(ColoredString text)
        {
            // Get height required for the hover window
            var height = CalculateTotalHeight(text.String);

            // Resize hover window
            Resize(_maxContentWidth + 2, height + 2, true);
            _innerSurface.Resize(_maxContentWidth, height, true);

            // Draw border on outer surface
            Surface.DrawBox(new Rectangle(0, 0, Width, Height), ShapeParameters.CreateStyledBox(ICellSurface.ConnectedLineThin, new ColoredGlyph(Color.Green, Color.Black)));

            _innerSurface.Surface.PrettyPrint(0, 0, text);
        }

        private int CalculateTotalHeight(string text)
        {
            string[] lines = text.Split('\n');
            int totalHeight = 0;

            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                int currentLineWidth = 0;

                foreach (string word in words)
                {
                    if (currentLineWidth + word.Length < _maxContentWidth)
                    {
                        currentLineWidth += word.Length;
                        if (currentLineWidth > 0 && currentLineWidth < _maxContentWidth)
                        {
                            currentLineWidth++; // Add space if not the first word on the line
                        }
                    }
                    else
                    {
                        totalHeight++; // Move to the next line
                        currentLineWidth = word.Length; // Start a new line with the word
                    }
                }

                totalHeight++; // Move to the next line for newline character '\n'
            }

            return totalHeight;
        }
    }
}
