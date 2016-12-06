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

            var dto = new ModelDto { BaseDate = model.BaseDate };

            if (model.Sub != null)
            {
                dto.SubProperName = model.Sub.ProperName;

                if (model.Sub.SubSub != null)
                {
                    dto.SubSubSubCoolProperty = model.Sub.SubSub.CoolProperty;
                }
            }

            if (model.Sub2 != null)
            {
                dto.Sub2ProperName = model.Sub2?.ProperName;
            }

            if (model.SubWithExtraName != null)
            {
                dto.SubWithExtraNameProperName = model.SubWithExtraName?.ProperName;
            }

            return dto;
        }
    }
}