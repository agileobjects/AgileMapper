namespace AgileObjects.AgileMapper.UnitTests
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
    public class WhenUnflatteningFromQueryStrings
    {
        [Fact]
        public void ShouldUnflattenToNestedProperties()
        {
            var queryString = "Value1=123&Value2%2EValue=2018-08-01".ToQueryString();
            var result = Mapper.Unflatten(queryString).To<PublicTwoFields<int, PublicField<DateTime>>>();

            result.ShouldNotBeNull();
            result.Value1.ShouldBe(123);
            result.Value2.ShouldNotBeNull();
            result.Value2.Value.ShouldBe(new DateTime(2018, 08, 01));
        }

        [Fact]
        public void ShouldUnflattenToACollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var queryString = "%5B0%5D%2EValue=1&%5B1%5D%2EValue=2&%5B2%5D%2EValue=3".ToQueryString();
                
                var result = mapper.Unflatten(queryString).To<PublicProperty<int>[]>();

                result.ShouldNotBeNull();
                result.Length.ShouldBe(3);
                result[0].Value.ShouldBe(1);
                result[1].Value.ShouldBe(2);
                result[2].Value.ShouldBe(3);
            }
        }
    }
}
