namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;

    internal class ComplexTypeFactory
    {
        public T Create<T>()
        {
            return Activator.CreateInstance<T>();
        }
    }
}