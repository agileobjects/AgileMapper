namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
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

            var resultDictionary = result
                .Split('&')
                .Select(data => data.Split('='))
                .Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            resultDictionary["SponsorName"].ShouldBe("Sherri");
            resultDictionary["Nums%5B0%5D"].ShouldBe("1");
            resultDictionary["Nums%5B1%5D"].ShouldBe("2");
            resultDictionary["Nums%5B2%5D"].ShouldBe("3");
            resultDictionary["Address%2ELine1"].ShouldBe("One%21");
            resultDictionary["Address%2ELine2"].ShouldBe("Two%21");
            resultDictionary["Products%5B0%5D%2EProductId"].ShouldBe("prod-1");
            resultDictionary["Products%5B1%5D%2EProductId"].ShouldBe("prod-2");
            resultDictionary["Products%5B2%5D%2EProductId"].ShouldBe("prod-3");
            resultDictionary["Products%5B0%5D%2EPrice"].ShouldBe("0%2E99");
            resultDictionary["Products%5B1%5D%2EPrice"].ShouldBe("1%2E99");
            resultDictionary["Products%5B2%5D%2EPrice"].ShouldBe("2%2E99");
        }
    }
}
