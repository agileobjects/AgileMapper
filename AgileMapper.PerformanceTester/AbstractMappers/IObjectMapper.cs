namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    internal interface IObjectMapper
    {
        string Name { get; }

        void Initialise();

        object Map();
    }
}
