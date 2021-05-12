namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringObjectTrackingInline
    {
        [Fact]
        public void ShouldSupportInlineDisabledObjectTracking()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var product = new Product();

                var resultProducts = mapper
                    .Map(new[] { product, product })
                    .ToANew<IEnumerable<ProductDto>>(cfg => cfg
                        .DisableObjectTracking());

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldNotBeSameAs(resultProducts.Second());
            }
        }

        [Fact]
        public void ShouldSupportInlineDisabledObjectTrackingWithGlobalIdentityIntegrity()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MaintainIdentityIntegrity();

                var product = new Product();

                var resultProducts = mapper
                    .Map(new[] { product, product })
                    .ToANew<IEnumerable<ProductDto>>(cfg => cfg
                        .DisableObjectTracking());

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldNotBeSameAs(resultProducts.Second());

                var anon = new { Name = "Who?!" };

                var resultAnons = mapper
                    .Map(new[] { anon, anon })
                    .ToANew<IEnumerable<PersonViewModel>>();

                resultAnons.Count().ShouldBe(2);
                resultAnons.First().Name.ShouldBe("Who?!");
                resultAnons.First().ShouldBeSameAs(resultAnons.Second());

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldSupportInlineIdentityIntegrity()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var product = new Product();

                var resultProducts = mapper
                    .Map(new[] { product, product })
                    .ToANew<IEnumerable<ProductDto>>(cfg => cfg.MaintainIdentityIntegrity());

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldBeSameAs(resultProducts.Second());
            }
        }

        [Fact]
        public void ShouldSupportInlineIdentityIntegrityWithGlobalDisabledObjectTracking()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.DisableObjectTracking();

                var product = new Product();

                var resultProducts = mapper
                    .Map(new[] { product, product })
                    .ToANew<IEnumerable<ProductDto>>(cfg => cfg.MaintainIdentityIntegrity());

                resultProducts.Count().ShouldBe(2);
                resultProducts.First().ShouldBeSameAs(resultProducts.Second());

                var anon = new { Name = "Who?!" };

                var resultAnons = mapper
                    .Map(new[] { anon, anon })
                    .ToANew<IEnumerable<PersonViewModel>>();

                resultAnons.Count().ShouldBe(2);
                resultAnons.First().Name.ShouldBe("Who?!");
                resultAnons.First().ShouldNotBeSameAs(resultAnons.Second());
            }
        }

        [Fact]
        public void ShouldErrorIfIdentityIntegrityAndDisabledObjectTrackingConfiguredInline()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.DeepClone(
                        new[] { new Customer { Name = "BOOM" } },
                        cfg => cfg
                            .MaintainIdentityIntegrity()
                            .And
                            .DisableObjectTracking());
                }
            });

            configEx.Message.ShouldContain("Object tracking cannot be disabled");
            configEx.Message.ShouldContain("Customer[] -> Customer[]");
            configEx.Message.ShouldContain("with identity integrity configured");
        }

        [Fact]
        public void ShouldErrorIfDisabledObjectTrackingAndIdentityIntegrityConfiguredInline()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.DeepClone(
                        new[] { new Customer { Name = "BOOM" } },
                        cfg => cfg
                            .DisableObjectTracking()
                            .And
                            .MaintainIdentityIntegrity());
                }
            });

            configEx.Message.ShouldContain("Identity integrity cannot be configured");
            configEx.Message.ShouldContain("Customer[] -> Customer[]");
            configEx.Message.ShouldContain("with object tracking disabled");
        }
    }
}
