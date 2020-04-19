namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
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
        public void ShouldApplyASequentialDataSourceToANestedComplexTypeMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue184.SourcePets>()
                    .ToANew<Issue184.TargetPets>()
                    .Map((src, _) => src.TheCat)
                    .Then.Map((src, _) => src.TheDog)
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
        public void ShouldApplyASequentialDataSourceToANestedComplexTypeMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue184.SourcePets>()
                    .ToANew<Issue184.TargetPets>()
                    .If((src, _) => src.TheCat.CatName.Length > 5)
                    .Map((src, _) => src.TheCat)
                    .Then.Map((src, _) => src.TheDog)
                    .To(tp => tp.PetNames);

                var nonMatchingSource = new Issue184.SourcePets
                {
                    TheCat = new Issue184.Cat { CatName = "Meow" },
                    TheDog = new Issue184.Dog { DogName = "Woof" }
                };

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Issue184.TargetPets>();

                nonMatchingResult.PetNames.ShouldNotBeNull();
                nonMatchingResult.PetNames.CatName.ShouldBeNull();
                nonMatchingResult.PetNames.DogName.ShouldBe("Woof");

                var matchingSource = new Issue184.SourcePets
                {
                    TheCat = new Issue184.Cat { CatName = "Tiddles" },
                    TheDog = new Issue184.Dog { DogName = "Rover" }
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<Issue184.TargetPets>();

                matchingResult.PetNames.ShouldNotBeNull();
                matchingResult.PetNames.CatName.ShouldBe("Tiddles");
                matchingResult.PetNames.DogName.ShouldBe("Rover");
            }
        }

        [Fact]
        public void ShouldApplyASequentialDataSourceToARootArray()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Address[], Address[]>>()
                    .ToANew<Address[]>()
                    .Map((src, _) => src.Value1)
                    .Then.Map((src, _) => src.Value2)
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
        public void ShouldApplyASequentialDataSourceToAListCtorParameter()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<IList<Address>, IList<Address>>>()
                    .ToANew<PublicCtor<IList<Address>>>()
                    .Map((src, _) => src.Value1)
                    .Then.Map((src, _) => src.Value2)
                    .ToCtor<IList<Address>>();

                var source = new PublicTwoFields<IList<Address>, IList<Address>>
                {
                    Value1 = new[] { new Address { Line1 = "Address 1" } },
                    Value2 = new[] { new Address { Line1 = "Address 2" } }
                };

                var result = mapper.Map(source).ToANew<PublicCtor<IList<Address>>>();

                result.Value.ShouldNotBeNull().Count.ShouldBe(2);
                result.Value.First().Line1.ShouldBe("Address 1");
                result.Value.Second().Line1.ShouldBe("Address 2");
            }
        }

        [Fact]
        public void ShouldApplyASequentialDataSourceToARootArrayConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Address[], Address[]>>()
                    .ToANew<Address[]>()
                    .Map((src, _) => src.Value1)
                    .Then
                        .If(ctx => ctx.Source.Value2.Length > 1)
                        .Map((src, _) => src.Value2)
                    .ToTarget();

                var nonMatchingSource = new PublicTwoFields<Address[], Address[]>
                {
                    Value1 = new[] { new Address { Line1 = "Address 1" } },
                    Value2 = new[] { new Address { Line1 = "Address 2" } }
                };

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Address[]>();

                nonMatchingResult.ShouldHaveSingleItem().Line1.ShouldBe("Address 1");

                var matchingSource = new PublicTwoFields<Address[], Address[]>
                {
                    Value1 = new[] { new Address { Line1 = "Address 1" } },
                    Value2 = new[]
                    {
                        new Address { Line1 = "Address 2" },
                        new Address { Line1 = "Address 3" }
                    }
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<Address[]>();

                matchingResult.Length.ShouldBe(3);
                matchingResult.First().Line1.ShouldBe("Address 1");
                matchingResult.Second().Line1.ShouldBe("Address 2");
                matchingResult.Third().Line1.ShouldBe("Address 3");
            }
        }

        [Fact]
        public void ShouldApplySequentialDataSourcesToANestedArrayConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var sourceType = new
                {
                    First = default(Address[]),
                    Second = default(Address[]),
                    Third = default(Address[])
                };

                mapper.WhenMapping
                    .From(sourceType)
                    .To<PublicField<Address[]>>()
                    .If((src, tgt, i) => -i.GetValueOrDefault() == 0)
                    .Map((src, _) => src.First)
                    .Then
                        .If((src, _) => src.Second[0].Line2 != null)
                        .Map((src, _) => src.Second)
                    .Then
                        .Map((src, _) => src.Third)
                    .To(pf => pf.Value);

                var nonMatchingSource = new
                {
                    First = new[] { new Address { Line1 = "Addr 1" } },
                    Second = new[] { new Address { Line1 = "Addr 2" } },
                    Third = new[] { new Address { Line1 = "Addr 3" } }
                };

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<Address[]>>();

                nonMatchingResult.Value.ShouldNotBeNull().Length.ShouldBe(2);
                nonMatchingResult.Value.First().Line1.ShouldBe("Addr 1");
                nonMatchingResult.Value.Second().Line1.ShouldBe("Addr 3");

                var matchingSource = new
                {
                    First = new[] { new Address { Line1 = "Addr 1" } },
                    Second = new[] { new Address { Line1 = "Addr 2.1", Line2 = "Addr 2.2" } },
                    Third = new[] { new Address { Line1 = "Addr 3" } }
                };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<Address[]>>();

                matchingResult.Value.ShouldNotBeNull().Length.ShouldBe(3);
                matchingResult.Value.First().Line1.ShouldBe("Addr 1");
                matchingResult.Value.Second().Line1.ShouldBe("Addr 2.1");
                matchingResult.Value.Second().Line2.ShouldBe("Addr 2.2");
                matchingResult.Value.Third().Line1.ShouldBe("Addr 3");
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
                    .Then.Map((src, _) => src.TheDog)
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
                        .Then.Map((src, _) => src.TheCat)
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
                        .Then.Map((src, _) => src.TheDog)
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
                        .Then.Map(ctx => ctx.Source.Value2)
                        .To(pp => pp.Value);
                }
            });

            configEx.Message.ShouldContain("PublicTwoFields<string, string>.Value2");
            configEx.Message.ShouldContain("cannot be sequentially applied");
            configEx.Message.ShouldContain("PublicProperty<DateTime>.Value");
            configEx.Message.ShouldContain("cannot have sequential data sources");
        }

        [Fact]
        public void ShouldErrorIfIgnoredSourceMemberSpecified()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .From<PublicTwoFields<Address, Address>>()
                        .ToANew<PublicProperty<Address>>()
                        .IgnoreSource(ptf => ptf.Value2)
                        .And
                        .Map((ptf, _) => ptf.Value1)
                        .Then.Map((ptf, _) => ptf.Value2)
                        .To(pp => pp.Value);
                }
            });

            configEx.Message.ShouldContain("PublicTwoFields<Address, Address>.Value2");
            configEx.Message.ShouldContain("PublicProperty<Address>.Value");
            configEx.Message.ShouldContain("conflicts with an ignored source member");
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
