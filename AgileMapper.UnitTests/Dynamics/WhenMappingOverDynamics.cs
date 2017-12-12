namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
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
    }
}
