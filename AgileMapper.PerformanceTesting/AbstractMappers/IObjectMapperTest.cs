namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System.Diagnostics;

    public interface IObjectMapperTest
    {
        string Type { get; }

        string Name { get; }

        int NumberOfExecutions { get; }

        void Initialise();

        object Execute(Stopwatch timer);

        void Verify(object result);
    }
}
