namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOnToStructMembers
    {
        [Fact]
        public void ShouldMapAMemberProperty()
        {
            var source = new PublicPropertyStruct<PublicTwoFieldsStruct<string, string>>
            {
                Value = new PublicTwoFieldsStruct<string, string>
                {
                    Value1 = "Over here!",
                    Value2 = "Yes, here!"
                }
            };

            var target = new PublicPropertyStruct<PublicTwoFieldsStruct<string, string>>
            {
                Value = new PublicTwoFieldsStruct<string, string>
                {
                    Value1 = "Over there!"
                }
            };

            var result = Mapper.Map(source).OnTo(target);

            result.Value.Value1.ShouldNotBe("Over here!");
            result.Value.Value1.ShouldBe("Over there!");
            result.Value.Value2.ShouldBe("Yes, here!");
        }
    }
}