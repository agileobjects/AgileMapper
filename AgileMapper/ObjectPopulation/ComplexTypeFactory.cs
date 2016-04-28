namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using Extensions;

    internal class ComplexTypeFactory
    {
        private readonly ICollection<Action<object>> _creationCallbacks;

        public ComplexTypeFactory()
        {
            _creationCallbacks = new List<Action<object>>();
        }

        public T Create<T>()
        {
            var instance = Activator.CreateInstance<T>();

            _creationCallbacks.Broadcast(instance);

            return instance;
        }

        internal void AddCreationCallback(Action<object> callback)
        {
            _creationCallbacks.Add(callback);
        }
    }
}