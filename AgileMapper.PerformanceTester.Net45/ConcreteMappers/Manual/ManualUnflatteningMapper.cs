namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using AbstractMappers;
    using static TestClasses.Flattening;

    internal class ManualUnflatteningMapper : UnflatteningMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ModelObject Unflatten(ModelDto dto)
        {
            return new ModelObject
            {
                BaseDate = dto.BaseDate,
                Sub = new ModelSubObject
                {
                    ProperName = dto.SubProperName,
                    SubSub = new ModelSubSubObject
                    {
                        CoolProperty = dto.SubSubSubCoolProperty
                    }
                },
                Sub2 = new ModelSubObject
                {
                    ProperName = dto.Sub2ProperName
                },
                SubWithExtraName = new ModelSubObject
                {
                    ProperName = dto.SubWithExtraNameProperName
                }
            };
        }
    }
}