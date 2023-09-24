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

        private readonly Lazy<ControlSurface> _controlSurface;
        private readonly Lazy<EffectHandler> _effectHandler;

        public ControlHost Controls 
        { 
            get 
            { 
                if (!_controlSurface.IsValueCreated)
                    Children.Insert(0, _controlSurface.Value);
                return _controlSurface.Value.Controls; 
            } 
        }

        public ControlSurface Surface
        {
            get
            {
                if (!_controlSurface.IsValueCreated)
                    Children.Insert(0, _controlSurface.Value);
                return _controlSurface.Value;
            }
        }

        public EffectHandler Effects 
        { 
            get 
            {
                if (!_effectHandler.IsValueCreated)
                    SadComponents.Add(_effectHandler.Value);
                return _effectHandler.Value; 
            } 
        }

        public Scene()
        {
            _controlSurface = new Lazy<ControlSurface>(() => new ControlSurface(Width, Height));
            _effectHandler = new Lazy<EffectHandler>(() => new EffectHandler());
        }
    }
}
