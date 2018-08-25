namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using static TestClasses.Flattening;

    internal abstract class UnflatteningMapperBase : MapperTestBase
    {
        private readonly ModelDto _modelDto;

        protected UnflatteningMapperBase()
        {
            _modelDto = new ModelDto
            {
                BaseDate = new DateTime(2007, 4, 5),
                SubProperName = "Some name",
                SubSubSubCoolProperty = "Cool daddy-o",
                Sub2ProperName = "Sub 2 name",
                SubWithExtraNameProperName = "Some other name"
            };
        }

        public override object Execute(Stopwatch timer) => Unflatten(_modelDto);

        protected abstract ModelObject Unflatten(ModelDto dto);

        public override void Verify(object result)
        {
            throw new NotImplementedException();
        }
    }
}