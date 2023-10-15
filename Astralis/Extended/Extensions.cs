using SadConsole;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Extended
{
    internal static class Extensions
    {
        public static IEnumerable<Point> GetCirclePositions(this Point center, int radius)
        {
            var coords = new List<Point>(radius * radius);
            for (int y = center.Y - radius; y <= center.Y + radius; y++)
            {
                for (int x = center.X - radius; x <= center.X + radius; x++)
                {
                    coords.Add(new Point(x, y));
                }
            }

            foreach (var coord in coords)
            {
                if (((coord.X - center.X) * (coord.X - center.X)) + ((coord.Y - center.Y) * (coord.Y - center.Y)) <= radius * radius)
                    yield return coord;
            }
        }

        public static IEnumerable<Point> GetSquarePositions(this Point center, int radius)
        {
            for (int x=center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    yield return new Point(x, y);
                }
            }
        }

        public static void PrettyPrint(this ICellSurface surface, int x, int y, ColoredString text)
        {
            if (string.IsNullOrWhiteSpace(text.String)) return;

            int width = surface.Width;
            int height = surface.Height;

            int startPosX = x;
            int startPosY = y;

            // Split the character array into parts based on cell width
            string[] splittedTextArray = text.String.WordWrap(width)
                .Select(a => a.TrimEnd())
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .ToArray();
            int coloredIndex = 0;

            int yIndex = 0;
            for (; y < height; y++)
            {
                // Don't go out of bounds of the cell height
                if (splittedTextArray.Length <= y)
                    break;

                // Print each array to the correct y index
                // Remove spaces in the front on the newline
                char[] textArr = splittedTextArray[y].SkipWhile(a => a == ' ').ToArray();

                int index = 0;
                foreach (char character in textArr)
                {
                    if (yIndex >= height)
                    {
                        y = height;
                        break;
                    }

                    var realIndex = (startPosY + yIndex) * width + (startPosX + index++);

                    if (!text.IgnoreGlyph)
                        surface[realIndex].Glyph = text[coloredIndex].GlyphCharacter;

                    if (!text.IgnoreBackground)
                        surface[realIndex].Background = text[coloredIndex].Background;

                    if (!text.IgnoreForeground)
                        surface[realIndex].Foreground = text[coloredIndex].Foreground;

                    if (!text.IgnoreMirror)
                        surface[realIndex].Mirror = text[coloredIndex].Mirror;

                    if (!text.IgnoreDecorators)
                        surface[index].Decorators = text[coloredIndex].Decorators;

                    coloredIndex++;
                }
                if (index != 0)
                {
                    yIndex++;
                    coloredIndex++;
                }
            }
        }

        public static T Random<T>(this IEnumerable<T> collection, Random random)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int count = 0;
            T selected = default;

            foreach (var item in collection)
            {
                count++;
                if (random.Next(count) == 0)
                {
                    selected = item;
                }
            }

            if (count == 0)
            {
                throw new InvalidOperationException("The collection is empty.");
            }

            return selected;
        }

        public static T RandomOrDefault<T>(this IEnumerable<T> collection, Random random)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            int count = 0;
            T selected = default;

            foreach (var item in collection)
            {
                count++;
                if (random.Next(count) == 0)
                {
                    selected = item;
                }
            }

            if (count == 0)
            {
                return default;
            }

            return selected;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
        }

        public static Color HexToColor(this string hex)
        {
            // Remove any leading "#" character
            var hexColor = hex.TrimStart('#');

            // Check if the hex string is valid
            if (hexColor.Length != 6)
            {
                throw new ArgumentException("Invalid hex color format. The string must be 6 characters long (RRGGBB).");
            }

            try
            {
                // Parse the hexadecimal values for Red, Green, and Blue
                int red = Convert.ToInt32(hexColor.Substring(0, 2), 16);
                int green = Convert.ToInt32(hexColor.Substring(2, 2), 16);
                int blue = Convert.ToInt32(hexColor.Substring(4, 2), 16);

                // Create and return the Color object
                return new Color(red, green, blue);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid hex color format.", ex);
            }
        }

        public static string ToHex(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public static void ClearDecorators(this ColoredGlyphBase coloredGlyphBase)
        {
            if (coloredGlyphBase.Decorators != null)
            {
                coloredGlyphBase.Decorators.Clear();
                CellDecoratorHelpers.Pool.Return(coloredGlyphBase.Decorators);
                coloredGlyphBase.Decorators = null;
            }
        }
    }
}
