namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.AgileMapper
{
    using AbstractMappers;
    using TestClasses;

    internal class AgileMapperUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return Mapper.Map(dto).ToANew<ModelObject>();
        }
    }
}