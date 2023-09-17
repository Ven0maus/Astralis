namespace Astralis.Extended
{
    internal interface IEffect
    {
        bool IsFinished { get; }
        void Update();
    }
}
