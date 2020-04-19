namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Configuration;
    using AgileMapper.Extensions.Internal;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    // See https://github.com/agileobjects/AgileMapper/issues/184
    public class WhenConfiguringSequentialDataSources
    {
        [Fact]
        public void ShouldApplySequentialDataSourcesToANestedComplexTypeMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue184.SourcePets>()
                    .ToANew<Issue184.TargetPets>()
                    .Map((src, _) => src.TheCat)
                    .To(tp => tp.PetNames)
                    .Then
                    .Map((src, _) => src.TheDog)
                    .To(tp => tp.PetNames);

                var source = new Issue184.SourcePets
                {
                    TheCat = new Issue184.Cat { CatName = "Tiddles" },
                    TheDog = new Issue184.Dog { DogName = "Rover" }
                };

                var result = mapper.Map(source).ToANew<Issue184.TargetPets>();

                result.PetNames.ShouldNotBeNull();
                result.PetNames.CatName.ShouldBe("Tiddles");
                result.PetNames.DogName.ShouldBe("Rover");
            }
        }

        [Fact]
        public void ShouldApplySequentialDataSourcesToARootArray()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Address[], Address[]>>()
                    .ToANew<Address[]>()
                    .Map((src, _) => src.Value1)
                    .ToTarget()
                    .Then
                    .Map((src, _) => src.Value2)
                    .ToTarget();

                var source = new PublicTwoFields<Address[], Address[]>
                {
                    Value1 = new[] { new Address { Line1 = "Address 1" } },
                    Value2 = new[]
                    {
                        new Address { Line1 = "Address 2" },
                        new Address { Line1 = "Address 3" }
                    }
                };

                var result = mapper.Map(source).ToANew<Address[]>();

                result.Length.ShouldBe(3);
                result.First().Line1.ShouldBe("Address 1");
                result.Second().Line1.ShouldBe("Address 2");
                result.Third().Line1.ShouldBe("Address 3");
            }
        }

        [Fact]
        public void ShouldHandleANullSequentialDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue184.SourcePets>()
                    .ToANew<Issue184.TargetPets>()
                    .Map((src, _) => src.TheCat)
                    .To(tp => tp.PetNames)
                    .Then
                    .Map((src, _) => src.TheDog)
                    .To(tp => tp.PetNames);

                var source = new Issue184.SourcePets
                {
                    TheDog = new Issue184.Dog { DogName = "Spot" }
                };

                var result = mapper.Map(source).ToANew<Issue184.TargetPets>();

                result.PetNames.ShouldNotBeNull();
                result.PetNames.CatName.ShouldBeNull();
                result.PetNames.DogName.ShouldBe("Spot");
            }
        }

        [Fact]
        public void ShouldErrorIfDuplicateSequentialDataSourceConfigured()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Issue184.SourcePets>()
                        .ToANew<Issue184.TargetPets>()
                        .Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames)
                        .Then
                        .Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames);
                }
            });

            configEx.Message.ShouldContain("already has configured data source");
            configEx.Message.ShouldContain("TheCat");
        }

        [Fact]
        public void ShouldErrorIfSequentialDataSourceMemberDuplicated()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<Issue184.SourcePets>()
                        .ToANew<Issue184.TargetPets>()
                        .Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames)
                        .Then
                        .Map((src, _) => src.TheDog)
                        .To(tp => tp.PetNames)
                        .And
                        .Map((src, _) => src.TheCat)
                        .To(tp => tp.PetNames);
                }
            });

            configEx.Message.ShouldContain("already has configured data source");
            configEx.Message.ShouldContain("TheCat");
        }

        [Fact]
        public void ShouldErrorIfSimpleTypeMemberSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<string, string>>()
                        .To<PublicProperty<DateTime>>()
                        .Map(ctx => ctx.Source.Value1)
                        .To(pp => pp.Value)
                        .Then
                        .Map(ctx => ctx.Source.Value2)
                        .To(pp => pp.Value);
                }
            });

            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value2");
            configEx.Message.ShouldContain("cannot be sequentially applied");
            configEx.Message.ShouldContain("PublicProperty<DateTime>.Value");
            configEx.Message.ShouldContain("cannot have sequential data sources");
        }

        #region Helper Members

        public static class Issue184
        {
            public class SourcePets
            {
                public Cat TheCat { get; set; }

                public Dog TheDog { get; set; }
            }

            public class Cat
            {
                public string CatName { get; set; }
            }

            public class Dog
            {
                public string DogName { get; set; }
            }

            public class TargetPets
            {
                public PetNames PetNames { get; set; }
            }

            public class PetNames
            {
                public string CatName { get; set; }

                public string DogName { get; set; }
            }
        }

        #endregion
    }
}
