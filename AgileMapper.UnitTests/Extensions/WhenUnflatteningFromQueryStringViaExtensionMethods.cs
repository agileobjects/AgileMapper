namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenUnflatteningFromQueryStringViaExtensionMethods
    {
        [Fact]
        public void ShouldUnflatten()
        {
            var result = "Line1=Here&Line2=There".ToQueryString().Unflatten().To<Address>();

            result.ShouldNotBeNull();
            result.Line1.ShouldBe("Here");
            result.Line2.ShouldBe("There");
        }
    }
}
