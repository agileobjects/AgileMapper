namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.ValueInjecter
{
    using AbstractMappers;
    using Omu.ValueInjecter;
    using Omu.ValueInjecter.Injections;
    using static TestClasses.Flattening;

    internal class ValueInjecterUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return (ModelObject)new ModelObject().InjectFrom<UnflatLoopInjection>(dto);
        }
    }
}