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
    public class WhenUnflatteningFromQueryStringsViaExtensionMethods
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
        public void ShouldHandleAZeroLengthKey()
        {
            var result = "Line1=Somewhere%20else&=Nowhere".ToQueryString().Unflatten().To<Address>();

            result.ShouldNotBeNull();
            result.Line1.ShouldBe("Somewhere else");
        }

        [Fact]
        public void ShouldHandleAZeroLengthValueUsingASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = "Line1=Nowhere&Line2="
                    .ToQueryString()
                    .UnflattenUsing(mapper)
                    .To<Address>(cfg => cfg.Map("Somewhere!").To(a => a.Line1));

                result.ShouldNotBeNull();
                result.Line1.ShouldBe("Somewhere!");
                result.Line2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldHandleAMisformattedValue()
        {
            var result = "Line1=NowhereLine2=&Line2&Line2=Hello&Line2=%20there&Line2&".ToQueryString().Unflatten().To<Address>();

            result.ShouldNotBeNull();
            result.Line1.ShouldBe("NowhereLine2=");
            result.Line2.ShouldBe("Hello, there");
        }

        [Fact]
        public void ShouldHandleNotAQueryString()
        {
            var result = "This is not a query string".ToQueryString().Unflatten().To<Address>();

            result.ShouldNotBeNull();
            result.Line1.ShouldBeNull();
            result.Line2.ShouldBeNull();
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
