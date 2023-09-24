using SadConsole;
using SadConsole.UI;

namespace Astralis.Extended.SadConsole
{
    /// <summary>
    /// A surface that supports controls, but has no mouse.
    /// </summary>
    internal class ControlSurface : ScreenSurface
    {
        public bool HasControls { get { return Controls != null; } }
        public ControlHost Controls { get; }

        public ControlSurface(int width, int height) : base(width, height)
        {
            Controls = new ControlHost();
            SadComponents.Add(Controls);
        }

        public void RemoveControlsLayer()
        {
            if (!HasControls) return;
            SadComponents.Remove(Controls);
        }

        public override string ToString() => "Surface (Controls)";
    }
}
