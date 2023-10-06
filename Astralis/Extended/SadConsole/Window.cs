using SadConsole;
using SadConsole.UI;
using SadConsole.UI.Controls;
using SadRogue.Primitives;
using System;

namespace Astralis.Extended.SadConsole
{
    internal static class ScWindow
    {
        public static void Message(string message, string closeButtonText, int width = 30, Action closedCallback = null, Colors colors = null)
        {
            width += 3;
            int buttonWidth = closeButtonText.Length + 2;

            var closeButton = new ButtonBox(buttonWidth, 3)
            {
                Text = closeButtonText,
            };

            int height = (int)Math.Ceiling(message.Length / (double)(width - 3));
            height += 2;

            var window = new Window(width, height + closeButton.Height)
            {
                Font = Game.Instance.Fonts["IBM_16x8"]
            };

            if (colors != null) window.Controls.ThemeColors = colors;

            window.WithinBorderPrint(1, 1, message);
            window.Title = "";

            closeButton.Position = new Point(window.Width - (closeButton.Width + 1), window.Height - 1 - closeButton.Height);
            closeButton.Click += (o, e) => { window.DialogResult = true; window.Hide(); closedCallback?.Invoke(); };

            window.Controls.Add(closeButton);
            closeButton.IsFocused = true;
            window.CloseOnEscKey = true;
            window.Show(true);
            window.Center();
        }

        /// <summary>
        /// Wraps within the window, making sure it adds enough space for the borders
        /// </summary>
        /// <param name="window"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="message"></param>
        private static void WithinBorderPrint(this Window window, int x, int y, string message)
        {
            var width = window.Width;
            int currentX = x, currentY = y;
            for (int i= 0; i < message.Length; i++)
            {
                if (currentX >= width - 2)
                {
                    currentX = 1;
                    currentY += 1;
                }

                var character = message[i];
                window.Surface[currentX, currentY].Glyph = character;
                currentX += 1;
            }
            window.Surface.IsDirty = true;
        }
    }
}
