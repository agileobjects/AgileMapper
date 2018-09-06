namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System;
    using System.Diagnostics;

    public abstract class MapperSetupTestBase : IObjectMapperTest
    {
        private readonly IObjectMapperTest _mapperTest;

        protected MapperSetupTestBase()
        {
            var type = GetType();
            var mapperTestName = $"{type.Namespace}.{type.Name.Replace("Setup", null)}";
            var mapperTestType = type.Assembly.GetType(mapperTestName) ?? throw new InvalidOperationException();

            _mapperTest = (IObjectMapperTest)Activator.CreateInstance(mapperTestType);

            _mapperTest.Initialise();
        }

        public abstract string Type { get; }

        public string Name => GetType().Name;

        public int NumberOfExecutions => 100;

        public abstract void Initialise();

        public object Execute(Stopwatch timer)
        {
            timer.Stop();

            Reset();

            timer.Start();

            Execute();

            var mapped = _mapperTest.Execute(new Stopwatch());

            _mapperTest.Verify(mapped);

            return null;
        }

        protected abstract void Execute();

        public void Verify(object result)
        {
        }

        protected abstract void Reset();
    }
}