namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System.Diagnostics;

    public abstract class MapperInstantiationBase : MapperTestBase
    {
        public override string Type => "new";

        public override int NumberOfExecutions => 10_000;

        public override object SourceObject => null;

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
