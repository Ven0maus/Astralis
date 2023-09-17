using System;

namespace Astralis.Extended
{
    internal interface IEffect
    {
        bool IsFinished { get; }
        Action OnFinished { get; set; }
        void Update();
    }
}
