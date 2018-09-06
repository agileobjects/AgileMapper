namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using static TestClasses.Ctor;

    public class ValueInjecterCtorMapper : CtorMapperBase
    {
        public override void Initialise()
            => Mapper.AddMap<ValueObject, ConstructedObject>(src => new ConstructedObject(src.Value));

        protected override ConstructedObject Construct(ValueObject valueObject)
            => Mapper.Map<ConstructedObject>(valueObject);
    }
}