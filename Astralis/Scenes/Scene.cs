using Astralis.Extended;
using SadConsole;
using SadConsole.UI;


namespace Astralis.Scenes
{
    internal abstract class Scene : ScreenObject
    {
        public string Name { get { return GetType().Name; } }
        public int Width { get { return Constants.ScreenWidth; } }
        public int Height { get { return Constants.ScreenHeight; } }

        private readonly ControlSurface _controlSurface;
        private readonly EffectHandler _effectHandler;

        public ControlHost Controls { get { return _controlSurface.Controls; } }
        public ControlSurface Surface { get { return _controlSurface; } }
        public EffectHandler Effects { get { return _effectHandler; } }

        public Scene()
        {
            _controlSurface = new ControlSurface(Width, Height);
            Children.Add(_controlSurface);

            _effectHandler = new EffectHandler();
            SadComponents.Add(_effectHandler);
        }
    }
}
