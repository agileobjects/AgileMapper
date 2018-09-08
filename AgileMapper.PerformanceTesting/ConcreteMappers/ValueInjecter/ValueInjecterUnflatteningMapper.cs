namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Flattening;

    public class ValueInjecterUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelObject Unflatten(ModelDto dto)
            => (ModelObject)new ModelObject().InjectFrom<UnflatLoopInjection>(dto);
    }
}