namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public abstract class MapperSetupTestBase : IObjectMapperTest
    {
        private readonly IObjectMapperTest _mapperTest;

        protected MapperSetupTestBase()
        {
            var mapperTestType = FindTestTypeOrThrow();

            _mapperTest = (IObjectMapperTest)Activator.CreateInstance(mapperTestType);
        }

        private Type FindTestTypeOrThrow()
        {
            var type = GetType();
            var testName = type.Name.Replace("Setup", null);
            var testFullName = $"{type.Namespace}.{testName}";

            var testType = type.Assembly.GetType(testFullName, throwOnError: false);

            if (testType != null)
            {
                return testType;
            }

            testType = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .FirstOrDefault(t => t.Name == testName);

            if (testType != null)
            {
                return testType;
            }

            throw new InvalidOperationException("Couldn't find mapper test " + testName);
        }

        public abstract string Type { get; }

        public string Name => GetType().Name;

        public int NumberOfExecutions => 100;

        public object SourceObject => _mapperTest.SourceObject;

        public abstract void Initialise();

        public object Execute(Stopwatch timer)
        {
            timer.Stop();

            Reset();

            timer.Start();

            var mapped = Execute();

            timer.Stop();

            _mapperTest.Verify(mapped);

            timer.Start();

            return null;
        }

        protected abstract object Execute();

        public void Verify(object result)
        {
        }

        protected abstract void Reset();
    }
}