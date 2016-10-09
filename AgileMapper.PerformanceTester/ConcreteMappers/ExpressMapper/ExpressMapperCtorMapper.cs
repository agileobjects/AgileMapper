using ExMapper = ExpressMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class ExpressMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            ExMapper
                .Register<ValueObject, ConstructedObject>()
                .Instantiate(vo => new ConstructedObject(vo.Value));

            ExMapper.Compile();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return ExMapper.Map<ValueObject, ConstructedObject>(valueObject);
        }
    }
}