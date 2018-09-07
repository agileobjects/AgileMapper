namespace AgileObjects.AgileMapper.PerformanceTesting.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using UnitTests.Common;
    using static TestClasses.Flattening;

    public abstract class FlatteningMapperBase : MapperTestBase
    {
        private readonly ModelObject _modelObject;

        protected FlatteningMapperBase()
        {
            _modelObject = new ModelObject
            {
                BaseDate = new DateTime(2007, 4, 5),
                Sub = new ModelSubObject
                {
                    ProperName = "Some name",
                    SubSub = new ModelSubSubObject
                    {
                        CoolProperty = "Cool daddy-o"
                    }
                },
                Sub2 = new ModelSubObject
                {
                    ProperName = "Sub 2 name"
                },
                SubWithExtraName = new ModelSubObject
                {
                    ProperName = "Some other name"
                },
            };
        }

        public override string Type => "flat";

        public override object SourceObject => _modelObject;

        public override object Execute(Stopwatch timer) => Flatten(_modelObject);

        protected abstract ModelDto Flatten(ModelObject model);

        public override void Verify(object result)
        {
            var dto = (result as ModelDto).ShouldNotBeNull();

            dto.BaseDate.ShouldBe(_modelObject.BaseDate);
            dto.SubProperName.ShouldBe("Some name");
            dto.SubSubSubCoolProperty.ShouldBe("Cool daddy-o");
            dto.Sub2ProperName.ShouldBe("Sub 2 name");
            dto.SubWithExtraNameProperName.ShouldBe("Some other name");
        }
    }
}