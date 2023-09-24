using Astralis.Extended;
using Astralis.Extended.Effects.Core;
using Astralis.Extended.SadConsole;
using SadConsole;
using SadConsole.UI;

namespace Astralis.Scenes
{
    internal abstract class Scene : ScreenObject
    {
        public readonly int Width;
        public readonly int Height;

        private string _name;
        public string Name { get { return _name ??= GetType().Name; } }

        private readonly Lazy<ControlSurface> _controlSurface;
        private readonly Lazy<EffectHandler> _effectHandler;

        public ControlHost Controls 
        { 
            get 
            {
                if (!Surface.HasControls) 
                    throw new System.Exception("Controls were removed from this scene.");
                return Surface.Controls; 
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
            Width = Constants.ScreenWidth;
            Height = Constants.ScreenHeight;
            _controlSurface = new Lazy<ControlSurface>(() => new ControlSurface(Width, Height));
            _effectHandler = new Lazy<EffectHandler>(() => new EffectHandler());
        }

        public void RemoveControlLayer()
        {
            Surface.RemoveControlsLayer();
        }
    }
}
