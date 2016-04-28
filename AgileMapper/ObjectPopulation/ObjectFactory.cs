namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;

    internal class ObjectFactory
    {
        public T Create<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}