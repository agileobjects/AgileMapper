namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringObjectTracking
    {
        [Fact]
        public void ShouldSupportSpecificDisabledObjectTrackingWithGlobalIdentityIntegrity()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .MaintainIdentityIntegrity()
                    .AndWhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .DisableObjectTracking();

                var product = new Product();
                var sourceProducts = new[] { product, product };
                var resultProducts = mapper.Map(sourceProducts).ToANew<IEnumerable<ProductDto>>();

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldNotBeSameAs(resultProducts.Second());

                var anon = new { Name = "Who?!" };
                var sourceAnons = new[] { anon, anon };
                var resultAnons = mapper.Map(sourceAnons).ToANew<IEnumerable<PersonViewModel>>();

                resultAnons.Count().ShouldBe(2);
                resultAnons.First().Name.ShouldBe("Who?!");
                resultAnons.First().ShouldBeSameAs(resultAnons.Second());
            }
        }

        [Fact]
        public void ShouldSupportSpecificIdentityIntegrityWithGlobalDisabledObjectTracking()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .DisableObjectTracking()
                    .AndWhenMapping
                    .From<Product>()
                    .To<ProductDto>()
                    .MaintainIdentityIntegrity();

                var product = new Product();
                var sourceProducts = new[] { product, product };
                var resultProducts = mapper.Map(sourceProducts).ToANew<IEnumerable<ProductDto>>();

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldBeSameAs(resultProducts.Second());

                var anon = new { Name = "Who?!" };
                var sourceAnons = new[] { anon, anon };
                var resultAnons = mapper.Map(sourceAnons).ToANew<IEnumerable<PersonViewModel>>();

                resultAnons.Count().ShouldBe(2);
                resultAnons.First().Name.ShouldBe("Who?!");
                resultAnons.First().ShouldNotBeSameAs(resultAnons.Second());
            }
        }
    }
}