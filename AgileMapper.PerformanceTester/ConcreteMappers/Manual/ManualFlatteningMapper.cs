namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using AbstractMappers;
    using TestClasses;

    internal class ManualFlatteningMapper : FlatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelDto Flatten(ModelObject model)
        {
            if (model == null)
            {
                return null;
            }

            return new ModelDto
            {
                BaseDate = model.BaseDate,
                SubProperName = model.Sub?.ProperName,
                Sub2ProperName = model.Sub2?.ProperName,
                SubWithExtraNameProperName = model.SubWithExtraName?.ProperName,
                SubSubSubCoolProperty = model.Sub?.SubSub?.CoolProperty
            };
        }
    }
}