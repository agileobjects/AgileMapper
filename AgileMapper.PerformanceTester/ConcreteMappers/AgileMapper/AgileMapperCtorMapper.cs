namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
            Mapper.GetPlanFor<ValueObject>().ToANew<ConstructedObject>();
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            return Mapper.Map(valueObject).ToANew<ConstructedObject>();
        }
    }
}