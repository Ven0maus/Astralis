using System.Threading;
using System;

namespace Astralis.Extended
{
    internal interface ILazy
    {
        bool IsValueCreated { get; }
        object Value { get; }
    }

    /// <summary>
    /// An extended version that has an interface ILazy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Lazy<T> : System.Lazy<T>, ILazy
    {
        object ILazy.Value => Value;

        public Lazy(bool isThreadSafe) : base(isThreadSafe) { }
        public Lazy(Func<T> valueFactory) : base(valueFactory) { }
        public Lazy(LazyThreadSafetyMode mode) : base(mode) { }
        public Lazy(T value) : base(value) { }
        public Lazy(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe) { }
        public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode) { }
    }
}
