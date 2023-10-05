using System;

namespace Astralis.Extended.Effects.Core
{
    internal interface IEffect
    {
        bool IsFinished { get; }
        Action OnFinished { get; set; }
        void Update(TimeSpan delta);
    }
}
