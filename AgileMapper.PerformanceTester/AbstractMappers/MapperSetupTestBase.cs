namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;

    internal abstract class MapperSetupTestBase : IObjectMapperTest
    {
        public string Name => GetType().Name;

        public int NumberOfExecutions => 100;

        public abstract void Initialise();

        public object Execute(Stopwatch timer)
        {
            timer.Stop();

            Reset();

            timer.Start();

            Execute();

            return null;
        }

        protected abstract void Reset();

        protected abstract void Execute();
    }
}