namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Flattening;

    public class ValueInjecterFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelDto Flatten(ModelObject model)
            => (ModelDto)new ModelDto().InjectFrom<FlatLoopInjection>(model);
    }
}