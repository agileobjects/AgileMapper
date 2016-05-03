namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Extensions;

    internal class ComplexTypeFactory
    {
        private readonly ICollection<ObjectCreationCallback> _creationCallbacks;

        public ComplexTypeFactory()
        {
            _creationCallbacks = new List<ObjectCreationCallback>();
        }

        public T Create<T>()
        {
            var instance = Activator.CreateInstance<T>();

            _creationCallbacks.ForEach((cb, i) => cb.CallbackIfAppropriate(instance));

            return instance;
        }

        internal void AddCreationCallback<TTarget>(Action<TTarget> callback)
        {
            var callbackObject = new ObjectCreationCallback(
                typeof(TTarget),
                o => callback.Invoke((TTarget)o));

            _creationCallbacks.Add(callbackObject);
        }

        private class ObjectCreationCallback
        {
            private readonly Type _targetType;
            private readonly Action<object> _callback;

            public ObjectCreationCallback(Type targetType, Action<object> callback)
            {
                _targetType = targetType;
                _callback = callback;
            }

            public void CallbackIfAppropriate<T>(T instance)
            {
                if (_targetType.IsAssignableFrom(typeof(T)))
                {
                    _callback.Invoke(instance);
                }
            }
        }
    }
}