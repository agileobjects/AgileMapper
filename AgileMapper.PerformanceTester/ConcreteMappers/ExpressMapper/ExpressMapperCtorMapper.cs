namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ExpressMapper
{
    using AbstractMappers;
    using global::ExpressMapper;
    using TestClasses;

    internal class ExpressMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            Mapper
                .Register<ValueObject, ConstructedObject>()
                .Instantiate(vo => new ConstructedObject(vo.Value));

            Mapper.Compile();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return Mapper.Map<ValueObject, ConstructedObject>(valueObject);
        }
    }
}