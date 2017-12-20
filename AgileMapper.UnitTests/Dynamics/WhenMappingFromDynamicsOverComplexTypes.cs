namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsOverComplexTypes
    {
        [Fact]
        public void ShouldOverwriteComplexTypeMembers()
        {
            dynamic source = new ExpandoObject();

            source.Line1 = "Up there!";
            source.Line2 = default(string);

            var target = new Address { Line2 = "Up where?!" };

            Mapper.Map(source).Over(target);

            target.Line1.ShouldBe("Up there!");
            target.Line2.ShouldBeNull();
        }
    }
}
