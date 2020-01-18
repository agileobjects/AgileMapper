namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToDynamics
    {
        [Fact]
        public void ShouldUpdateANullMemberValue()
        {
            var source = new PublicTwoFieldsStruct<string, string>
            {
                Value1 = "New value!",
                Value2 = "Won't be a new value!"
            };

            dynamic target = new ExpandoObject();
            target.Value1 = default(string);
            target.Value2 = "Already populated!";

            Mapper.Map(source).OnTo(target);

            Assert.Equal("New value!", target.Value1);
            Assert.Equal("Already populated!", target.Value2);
        }
    }
}
