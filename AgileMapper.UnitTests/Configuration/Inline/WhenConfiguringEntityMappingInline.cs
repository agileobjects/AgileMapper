﻿namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringEntityMappingInline
    {
        [Fact]
        public void ShouldMapEntityKeys()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Id = 999, Price = 99.99 };
                var result = mapper.Map(source).ToANew<ProductEntity>(c => c.MapEntityKeys());

                result.Id.ShouldBe(999);
                result.Price.ShouldBe(99.99);
            }
        }

        [Fact]
        public void ShouldIgnoreEntityKeysForASpecificMapping()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MapEntityKeys();

                var source = new { Id = 987, Name = "Barney" };

                var defaultResult = mapper.Map(source).ToANew<CategoryEntity>();

                defaultResult.Id.ShouldBe(987);
                defaultResult.Name.ShouldBe("Barney");

                var ignoreKeysResult = mapper.Map(source).ToANew<CategoryEntity>(c => c.IgnoreEntityKeys());

                ignoreKeysResult.Id.ShouldBeDefault();
                ignoreKeysResult.Name.ShouldBe("Barney");
            }
        }

        [Fact]
        public void ShouldErrorIfRedundantMapKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.MapEntityKeys();
                    mapper.Map(new ProductEntity()).ToANew<ProductEntity>(c => c.MapEntityKeys());
                }
            });

            configEx.Message.ShouldContain("already enabled");
        }

        [Fact]
        public void ShouldErrorIfRedundantIgnoreKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.Map(new ProductEntity()).ToANew<ProductEntity>(c => c.IgnoreEntityKeys());
                }
            });

            configEx.Message.ShouldContain("disabled by default");
        }
    }
}
