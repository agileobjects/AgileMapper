namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using TestClasses;

    internal class ValueInjecterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            Mapper.AddMap<ValueObject, ConstructedObject>(src => new ConstructedObject(src.Value));
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return Mapper.Map<ConstructedObject>(valueObject);
        }
    }
}