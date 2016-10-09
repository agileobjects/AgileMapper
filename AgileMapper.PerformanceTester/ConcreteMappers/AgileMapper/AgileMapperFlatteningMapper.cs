namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
            Mapper.GetPlanFor<ModelObject>().ToANew<ModelDto>();
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return Mapper.Map(model).ToANew<ModelDto>();
        }
    }
}