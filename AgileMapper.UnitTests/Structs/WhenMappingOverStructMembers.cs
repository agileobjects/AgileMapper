namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    [Trait("Category", "Checked")]
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

        [Fact]
        public void ShouldHandleNoMatchingSourceMember()
        {
            var source = new { Value1 = "You" };
            var target = new PublicTwoFieldsStruct<string, int> { Value1 = "kjd", Value2 = 527 };
            var result = Mapper.Map(source).Over(target);

            result.Value1.ShouldBe("You");  
            result.Value2.ShouldBe(527);
        }
    }
}