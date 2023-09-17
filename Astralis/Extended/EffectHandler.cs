using SadConsole;
using SadConsole.Components;
using SadConsole.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Astralis.Extended
{
    internal class EffectHandler : IComponent
    {
        public uint SortOrder => 0;
        public bool IsUpdate => true;
        public bool IsRender => false;
        public bool IsMouse => false;
        public bool IsKeyboard => false;

        private List<IEffect> _effects = new();

        public void OnAdded(IScreenObject host)
        {
            _effects = new();
        }

        public void OnRemoved(IScreenObject host)
        {
            _effects = null;
        }

        public void Add(IEffect effect)
        {
            if (_effects == null)
                throw new Exception("EffectHandler must be assigned as a component.");
            _effects.Add(effect);
        }

        public void Remove(IEffect effect)
        {
            if (_effects == null)
                throw new Exception("EffectHandler must be assigned as a component.");
            _effects.Remove(effect);
        }

        public void ProcessKeyboard(IScreenObject host, Keyboard keyboard, out bool handled) => throw new NotImplementedException();
        public void ProcessMouse(IScreenObject host, MouseScreenObjectState state, out bool handled) => throw new NotImplementedException();
        public void Render(IScreenObject host, TimeSpan delta) => throw new NotImplementedException();
        public void Update(IScreenObject host, TimeSpan delta)
        {
            if (_effects.Any())
            {
                _effects.ForEach(a =>
                {
                    a.Update();
                });
                _effects.RemoveAll(a => a.IsFinished);
            }
        }
    }
}
