namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverStructMembers
    {
        [Fact]
        public void ShouldOverwriteAMemberToDefault()
        {
            var source = new PublicProperty<PublicField<string>> { Value = null };
            var target = new PublicProperty<PublicPropertyStruct<string>>
            {
                Value = new PublicPropertyStruct<string> { Value = "Gone" }
            };

            var result = Mapper.Map(source).Over(target);

            result.ShouldBeSameAs(target);
            result.Value.ShouldBeDefault();
        }
    }
}