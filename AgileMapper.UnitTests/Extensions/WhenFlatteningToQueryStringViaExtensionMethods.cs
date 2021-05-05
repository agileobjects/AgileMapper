namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Collections.Generic;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenFlatteningToQueryStringViaExtensionMethods
    {
        [Fact]
        public void ShouldFlatten()
        {
            var source = new Address { Line1 = "Here", Line2 = "There" };
            var result = source.Flatten().ToQueryString();

            result.ShouldBe("Line1=Here&Line2=There");
        }

        [Fact]
        public void ShouldFlattenWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var callbackCalled = false;

                mapper.After.MappingEnds.Call(ctx => callbackCalled = true);

                var source = new Address { Line1 = "Here", Line2 = "There" };
                var result = source.FlattenUsing(mapper).ToQueryString();

                callbackCalled.ShouldBeTrue();
                result.ShouldBe("Line1=Here&Line2=There");
            }
        }

        [Fact]
        public void ShouldEncodeValues()
        {
            var source = new Address { Line1 = "He&re", Line2 = "Th=re" };
            var result = source.Flatten().ToQueryString();

            result.ShouldBe("Line1=He%26re&Line2=Th%3Dre");
        }

        [Fact]
        public void ShouldEncodeKeys()
        {
            var source = new Dictionary<string, string>
            {
                ["Valu&1"] = "Value 1!",
                ["Valu&2"] = "Value 2!"
            };
            var result = source.Flatten().ToQueryString();

            result.ShouldBe("Valu%261=Value%201%21&Valu%262=Value%202%21");
        }

        [Fact]
        public void ShouldFlattenWithInlineConfiguration()
        {
            var source = new
            {
                Name = "Sherri",
                Numbers = new[] { 1, 2, 3 },
                Address = new Address { Line1 = "One!", Line2 = "Two!" },
                Products = new List<Product>
                {
                    new Product { ProductId = "prod-1", Price = 0.99 },
                    new Product { ProductId = "prod-2", Price = 1.99 },
                    new Product { ProductId = "prod-3", Price = 2.99 }
                }
            };

            var result = source.Flatten().ToQueryString(cfg => cfg
                .ForDictionaries
                .MapMember(s => s.Name)
                .ToFullKey("SponsorName")
                .And
                .MapMember(s => s.Numbers)
                .ToMemberNameKey("Nums"));

            var resultDictionary = (QueryString)result;

            resultDictionary["SponsorName"].ShouldBe("Sherri");
            resultDictionary["Nums[0]"].ShouldBe("1");
            resultDictionary["Nums[1]"].ShouldBe("2");
            resultDictionary["Nums[2]"].ShouldBe("3");
            resultDictionary["Address.Line1"].ShouldBe("One!");
            resultDictionary["Address.Line2"].ShouldBe("Two!");
            resultDictionary["Products[0].ProductId"].ShouldBe("prod-1");
            resultDictionary["Products[1].ProductId"].ShouldBe("prod-2");
            resultDictionary["Products[2].ProductId"].ShouldBe("prod-3");
            resultDictionary["Products[0].Price"].ShouldBe("0.99");
            resultDictionary["Products[1].Price"].ShouldBe("1.99");
            resultDictionary["Products[2].Price"].ShouldBe("2.99");
        }
    }
}
