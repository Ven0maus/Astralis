using SadConsole.UI;
using SadRogue.Primitives;

namespace Astralis.Extended.SadConsole
{
    internal static class ColorsExtensions
    {
        public static void SetBackground(this Colors theme, Color color)
        {
            theme.ControlBackgroundNormal.SetColor(color);
            theme.ControlBackgroundSelected.SetColor(color);
            theme.ControlBackgroundFocused.SetColor(color);
            theme.ControlBackgroundMouseOver.SetColor(color);
            theme.ControlBackgroundMouseDown.SetColor(color);
        }

        public static void SetForeground(this Colors theme, Color color)
        {
            theme.ControlForegroundNormal.SetColor(color);
            theme.ControlForegroundSelected.SetColor(color);
            theme.ControlForegroundFocused.SetColor(color);
            theme.ControlForegroundMouseOver.SetColor(color);
            theme.ControlForegroundMouseDown.SetColor(color);
        }
    }
}
