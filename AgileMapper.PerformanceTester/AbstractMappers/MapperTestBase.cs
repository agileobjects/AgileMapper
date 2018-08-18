namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;

    internal abstract class MapperTestBase : IObjectMapperTest
    {
        public string Name => GetType().Name;

        public virtual int NumberOfExecutions => 1_000_000;

        public abstract void Initialise();

        public abstract object Execute(Stopwatch timer);
    }
}