namespace AgileObjects.AgileMapper.UnitTests.Configuration
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
    public class WhenConfiguringEntityMapping
    {
        [Fact]
        public void ShouldMapEntityKeys()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.MapEntityKeys();

                var source = new { Id = 456 };
                var result = mapper.Map(source).ToANew<ProductEntity>();

                result.Id.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldMapEntityKeysForASpecificTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<ProductEntity>()
                    .MapEntityKeys();

                var source = new { Id = 123 };
                var matchingResult = mapper.Map(source).ToANew<ProductEntity>();

                matchingResult.Id.ShouldBe(123);

                var nonMatchingTargetResult = mapper.Map(source).ToANew<OrderEntity>();

                nonMatchingTargetResult.Id.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreEntityKeysForASpecificSourceAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Id = 999 };

                mapper.WhenMapping
                    .MapEntityKeys()
                    .AndWhenMapping
                    .From(source).To<OrderEntity>()
                    .IgnoreEntityKeys();

                var matchingResult = mapper.Map(source).ToANew<OrderEntity>();

                matchingResult.Id.ShouldBeDefault();

                var nonMatchingSourceResult = mapper.Map(new { Id = 987, Name = "Fred" }).ToANew<CategoryEntity>();

                nonMatchingSourceResult.Id.ShouldBe(987);

                var nonMatchingTargetResult = mapper.Map(source).ToANew<CategoryEntity>();

                nonMatchingTargetResult.Id.ShouldBe(999);
            }
        }

        [Fact]
        public void ShouldErrorIfDuplicateMapKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<ProductDto>().To<ProductEntity>()
                        .MapEntityKeys()
                        .And
                        .MapEntityKeys();
                }
            });

            configEx.Message.ShouldContain("already enabled");
        }

        [Fact]
        public void ShouldErrorIfDuplicateIgnoreKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .MapEntityKeys()
                        .AndWhenMapping
                        .From<OrderDto>().To<OrderEntity>()
                        .IgnoreEntityKeys()
                        .And
                        .IgnoreEntityKeys();
                }
            });

            configEx.Message.ShouldContain("already disabled");
        }

        [Fact]
        public void ShouldErrorIfRedundantMapKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .MapEntityKeys()
                        .AndWhenMapping
                        .From<ProductEntity>().To<ProductEntity>()
                        .MapEntityKeys();
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
                    mapper.WhenMapping.To<ProductEntity>().IgnoreEntityKeys();
                }
            });

            configEx.Message.ShouldContain("disabled by default");
        }

        [Fact]
        public void ShouldErrorIfConflictingMapKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .MapEntityKeys()
                        .AndWhenMapping
                        .To<CategoryEntity>()
                        .IgnoreEntityKeys()
                        .And
                        .MapEntityKeys();
                }
            });

            configEx.Message.ShouldContain("already been disabled");
        }

        [Fact]
        public void ShouldErrorIfConflictingIgnoreKeysConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .To<ProductEntity>()
                        .MapEntityKeys()
                        .And
                        .IgnoreEntityKeys();
                }
            });

            configEx.Message.ShouldContain("already been enabled");
        }
    }
}
