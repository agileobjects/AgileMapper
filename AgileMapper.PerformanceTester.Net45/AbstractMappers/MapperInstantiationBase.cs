namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;

    internal abstract class MapperInstantiationBase : MapperTestBase
    {
        public override int NumberOfExecutions => 10_000;

        public override void Initialise()
        {
        }

        public override object Execute(Stopwatch timer) => CreateMapperInstance();

        protected abstract object CreateMapperInstance();

        public override void Verify(object result)
        {
        }
    }
}
