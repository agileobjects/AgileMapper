namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System.Diagnostics;
    using UnitTests.Common;
    using static TestClasses.Ctor;

    internal abstract class CtorMapperBase : MapperTestBase
    {
        private readonly ValueObject _valueObject;

        protected CtorMapperBase()
        {
            _valueObject = new ValueObject { Value = 5 };
        }

        public override object Execute(Stopwatch timer) => Construct(_valueObject);

        protected abstract ConstructedObject Construct(ValueObject valueObject);

        public override void Verify(object result)
        {
            var ctor = (result as ConstructedObject).ShouldNotBeNull();

            ctor.Value.ShouldBe(5);
        }
    }
}