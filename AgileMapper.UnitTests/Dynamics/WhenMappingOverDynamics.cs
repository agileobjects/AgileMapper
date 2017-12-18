namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverDynamics
    {
        [Fact]
        public void ShouldOverwriteASimpleTypeProperty()
        {
            var source = new { Value = 123 };

            dynamic target = new ExpandoObject();

            target.Value = 456;

            Mapper.Map(source).Over(target);

            ((int)target.Value).ShouldBe(123);
        }

        [Fact]
        public void ShouldOverwriteAnEnumProperty()
        {
            var source = new PublicPropertyStruct<TitleShortlist?>
            {
                Value = TitleShortlist.Mrs
            };

            dynamic target = new ExpandoObject();

            target.Value = Title.Mr;

            Mapper.Map(source).Over(target);

            ((TitleShortlist)target.Value).ShouldBe(TitleShortlist.Mrs);
        }

        [Fact]
        public void ShouldOverwriteFromAStructCollection()
        {
            var source = new[]
            {
                new PublicPropertyStruct<int> { Value = 1 },
                new PublicPropertyStruct<int> { Value = 2 },
                new PublicPropertyStruct<int> { Value = 3 },
            };

            dynamic target = new ExpandoObject();

            target._0__Value = 10;
            target._2__Value = 30;

            Mapper.Map(source).Over(target);

            ((IDictionary<string, object>)target).Count.ShouldBe(3);

            ((int)target._0__Value).ShouldBe(1);
            ((int)target._1__Value).ShouldBe(2);
            ((int)target._2__Value).ShouldBe(3);
        }
    }
}
