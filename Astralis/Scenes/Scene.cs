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

        private readonly Lazy<EffectHandler> _effectHandler;

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
            _effectHandler = new Lazy<EffectHandler>(() => new EffectHandler());
        }
    }
}
