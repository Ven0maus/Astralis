using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace Astralis.Extended
{
    internal static class Extensions
    {
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
