namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenFlatteningToQueryStringViaExtensionMethods
    {
        [Fact]
        public void ShouldFlattenToQueryString()
        {
            var source = new Address { Line1 = "Here", Line2 = "There" };
            var result = source.Flatten().ToQueryString();

            result.ShouldBe("Line1=Here&Line2=There");
        }

        [Fact]
        public void ShouldFlattenToQueryStringWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var callbackCalled = false;

                mapper.After.MappingEnds.Call(ctx => callbackCalled = true);

                var source = new Address { Line1 = "Here", Line2 = "There" };
                var result = source.Flatten(_ => _.Using(mapper)).ToQueryString();

                callbackCalled.ShouldBeTrue();
                result.ShouldBe("Line1=Here&Line2=There");
            }
        }
    }
}
