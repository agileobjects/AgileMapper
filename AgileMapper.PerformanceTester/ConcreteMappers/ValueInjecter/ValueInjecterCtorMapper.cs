using ViMapper = Omu.ValueInjecter.Mapper;

namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using TestClasses;

    internal class ValueInjecterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            ViMapper.AddMap<ValueObject, ConstructedObject>(src => new ConstructedObject(src.Value));
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return ViMapper.Map<ConstructedObject>(valueObject);
        }
    }
}