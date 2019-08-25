namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByValueFilter
    {
        [Fact]
        public void ShouldGloballyIgnoreStringSourceMemberByTypedValueCondition()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreSources(c => c.If<string>(str => str.IsNullOrWhiteSpace()));

                var source = new PublicTwoFields<int, string> { Value1 = 123, Value2 = "456" };
                var result = mapper.Map(source).ToANew<PublicTwoParamCtor<string, int>>();

                result.ShouldNotBeNull();
                result.Value1.ShouldBe("123");
                result.Value2.ShouldBeDefault();
            }
        }
    }
}
