using SadConsole;
using SadConsole.UI;

namespace Astralis.Extended
{
    /// <summary>
    /// A surface that supports controls, but has no mouse.
    /// </summary>
    internal class ControlSurface : ScreenSurface
    {
        public ControlHost Controls { get; }

        public ControlSurface(int width, int height) : base(width, height)
        {
            Controls = new ControlHost();
            SadComponents.Add(Controls);
        }

        public override string ToString() => "Surface (Controls)";
    }
}
