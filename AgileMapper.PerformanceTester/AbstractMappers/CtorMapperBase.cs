namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using TestClasses;

    internal abstract class CtorMapperBase : IObjectMapper
    {
        public string Name => GetType().Name;

        public abstract void Initialise();

        public object Map()
        {
            return Construct(new ValueObject { Value = 5 });
        }

        protected abstract ConstructedObject Construct(ValueObject valueObject);
    }
}