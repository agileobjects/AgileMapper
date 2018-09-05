namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;

    internal interface IObjectMapperTest
    {
        string Name { get; }

        int NumberOfExecutions { get; }

        void Initialise();

        object Execute(Stopwatch timer);

        void Verify(object result);
    }
}
