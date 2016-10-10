namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using global::AutoMapper;
    using TestClasses;

    internal class AutoMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            Mapper.Initialize(cfg => cfg.CreateMap<ValueObject, ConstructedObject>());
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return Mapper.Map<ValueObject, ConstructedObject>(valueObject);
        }
    }
}