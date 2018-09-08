namespace AgileObjects.AgileMapper.PerformanceTester.Net45.ConcreteMappers.ExpressMapper
{
    using global::ExpressMapper;
    using PerformanceTesting.AbstractMappers;
    using static PerformanceTesting.TestClasses.Ctor;

    public class ExpressMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<ValueObject, ConstructedObject>()
                .Instantiate(vo => new ConstructedObject(vo.Value));

            Mapper.Compile();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
            => Mapper.Map<ValueObject, ConstructedObject>(valueObject);
    }
}