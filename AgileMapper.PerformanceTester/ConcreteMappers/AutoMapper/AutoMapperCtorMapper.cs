using AmMapper = AutoMapper.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AutoMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AutoMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            AmMapper.Initialize(cfg => cfg.CreateMap<ValueObject, ConstructedObject>());
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return AmMapper.Map<ValueObject, ConstructedObject>(valueObject);
        }
    }
}