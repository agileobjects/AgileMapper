namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;
    using TestClasses;

    internal abstract class CtorMapperBase : MapperTestBase
    {
        public override object Execute(Stopwatch timer)
        {
            return Construct(new ValueObject { Value = 5 });
        }

        protected abstract ConstructedObject Construct(ValueObject valueObject);
    }
}