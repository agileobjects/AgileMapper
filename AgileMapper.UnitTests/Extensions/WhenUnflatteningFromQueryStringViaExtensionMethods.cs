namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System;
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

        [Fact]
        public void ShouldHandleALeadingQuestionMark()
        {
            var result = "?Line1=Some%20Place".ToQueryString().Unflatten().To<Address>();

            result.ShouldNotBeNull();
            result.Line1.ShouldBe("Some Place");
        }

        [Fact]
        public void ShouldErrorIfNullStringUsed()
        {
            Should.Throw<ArgumentException>(() => default(string).ToQueryString());
        }

        [Fact]
        public void ShouldErrorIfEmptyStringUsed()
        {
            Should.Throw<ArgumentException>(() => string.Empty.ToQueryString());
        }

        [Fact]
        public void ShouldErrorIfBlankStringUsed()
        {
            Should.Throw<ArgumentException>(() => "   ".ToQueryString());
        }
    }
}
