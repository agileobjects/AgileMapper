namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System.Diagnostics;

    public abstract class MapperTestBase : IObjectMapperTest
    {
        public abstract string Type { get; }

        public string Name => GetType().Name;

        public virtual int NumberOfExecutions => 1_000_000;

        public abstract void Initialise();

        public abstract object Execute(Stopwatch timer);
        
        public abstract void Verify(object result);
    }
}