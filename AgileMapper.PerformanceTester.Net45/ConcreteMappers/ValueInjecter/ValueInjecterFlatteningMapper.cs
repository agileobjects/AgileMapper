namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Flattening;

    internal class ValueInjecterFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            return (ModelDto)new ModelDto().InjectFrom<FlatLoopInjection>(model);
        }
    }
}