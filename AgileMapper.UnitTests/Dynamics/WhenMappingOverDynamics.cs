namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
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
    }
}
