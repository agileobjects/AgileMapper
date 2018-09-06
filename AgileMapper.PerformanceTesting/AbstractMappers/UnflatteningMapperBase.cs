namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using UnitTests.Common;
    using static TestClasses.Flattening;

    public abstract class UnflatteningMapperBase : MapperTestBase
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

        public override string Type => "unflat";

        public override object Execute(Stopwatch timer) => Unflatten(_modelDto);

        protected abstract ModelObject Unflatten(ModelDto dto);

        public override void Verify(object result)
        {
            var model = (result as ModelObject).ShouldNotBeNull();

            model.BaseDate.ShouldBe(_modelDto.BaseDate);

            var modelSub = model.Sub.ShouldNotBeNull();
            modelSub.ProperName.ShouldBe("Some name");

            var modelSubSub = modelSub.SubSub.ShouldNotBeNull();
            modelSubSub.CoolProperty.ShouldBe("Cool daddy-o");

            var modelSub2 = model.Sub2.ShouldNotBeNull();
            modelSub2.ProperName.ShouldBe("Sub 2 name");

            var modelExtra = model.SubWithExtraName.ShouldNotBeNull();
            modelExtra.ProperName.ShouldBe("Some other name");
        }
    }
}