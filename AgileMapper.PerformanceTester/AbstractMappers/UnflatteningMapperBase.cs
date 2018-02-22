namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using TestClasses;

    internal abstract class UnflatteningMapperBase : MapperTestBase
    {
        public override object Execute(Stopwatch timer)
        {
            return Unflatten(new ModelDto
            {
                BaseDate = new DateTime(2007, 4, 5),
                SubProperName = "Some name",
                SubSubSubCoolProperty = "Cool daddy-o",
                Sub2ProperName = "Sub 2 name",
                SubWithExtraNameProperName = "Some other name"
            });
        }

        protected abstract ModelObject Unflatten(ModelDto dto);
    }
}