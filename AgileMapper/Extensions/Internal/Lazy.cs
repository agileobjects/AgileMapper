#if NET35
namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;

    internal class Lazy<T>
    {
        private readonly object _syncLock;
        private readonly Func<T> _valueFactory;
        private bool _valueCreated;
        private T _value;

        public Lazy(Func<T> valueFactory)
        {
            _syncLock = new object();
            _valueFactory = valueFactory;
        }

        public T Value
        {
            get
            {
                if (_valueCreated)
                {
                    return _value;
                }

                lock (_syncLock)
                {
                    if (_valueCreated)
                    {
                        return _value;
                    }

                    _valueCreated = true;
                    return _value = _valueFactory.Invoke();
                }
            }
        }
    }
}
#endif