namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
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
    public class WhenConfiguringTypeIdentifiers
    {
        [Fact]
        public void ShouldUseAConfiguredIdentifier()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .InstancesOf<Person>()
                    .IdentifyUsing(p => p.Name);

                mapper.WhenMapping
                    .InstancesOf<PersonViewModel>()
                    .IdentifyUsing(p => p.Name);

                var source = new[]
                {
                    new Person { Id = Guid.NewGuid(), Name = "Boris", Address = new Address { Line1 = "My House" } }
                };
                var target = new List<PersonViewModel>
                {
                    new() { Id = Guid.NewGuid(), Name = "Boris" }
                };
                var result = mapper.Map(source).OnTo(target);

                result.ShouldHaveSingleItem();
                result.First().AddressLine1.ShouldBe("My House");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredIdentifierForADerivedType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .InstancesOf<Person>()
                    .IdentifyUsing(p => p.Name);

                var source = new[]
                {
                    new Customer { Title = Title.Dr, Name = "Hetty" }
                };
                var target = new List<Person> { new() { Title = Title.Ms, Name = "Hetty" } };
                var result = mapper.Map(source).Over(target);

                result.ShouldHaveSingleItem();
                result.First().Title.ShouldBe(Title.Dr);
            }
        }

        [Fact]
        public void ShouldUseAConfiguredIdentifierExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .InstancesOf<Person>()
                    .IdentifyUsing(p => p.Title + " " + p.Name);

                mapper.WhenMapping
                    .InstancesOf<PersonViewModel>()
                    .IdentifyUsing(p => p.Name);

                var source = new List<Person>
                {
                    new() { Title = Title.Dr, Name = "Boris", Address = new Address { Line1 = "Here" } }
                };
                var target = new List<PersonViewModel>
                {
                    new() { Name = "Dr Boris" }
                };
                var result = mapper.Map(source).OnTo(target);

                result.ShouldHaveSingleItem();
                result.First().AddressLine1.ShouldBe("Here");
            }
        }

        [Fact]
        public void ShouldUseACompositeIdentifierWithAnEntityKey()
        {
            using var mapper = Mapper.CreateNew();

            mapper.WhenMapping
                .InstancesOf<PublicTwoFields<int, Product>>()
                .IdentifyUsing(ptf => ptf.Value1, ptf => ptf.Value2);

            var source = new[]
            {
                new PublicTwoFields<int, Product>
                {
                    Value1 = 123,
                    Value2 = new Product { ProductId = "321", Price = 99.99 }
                },
                new PublicTwoFields<int, Product>
                {
                    Value1 = 456,
                    Value2 = new Product { ProductId = "654", Price = 11.99 }
                }
            };

            var target = new List<PublicTwoFields<int, Product>>
            {
                new()
                {
                    Value1 = 123,
                    Value2 = new Product { ProductId = "333", Price = 10.99 }
                },
                new()
                {
                    Value1 = 456,
                    Value2 = new Product { ProductId = "654", Price = 10.99 }
                }
            };

            var itemOne = target.First();
            var itemTwo = target.Second();

            mapper.Map(source).Over(target);

            target.Count.ShouldBe(2);

            target.First().ShouldBeSameAs(itemTwo);
            target.First().Value1.ShouldBe(456);
            target.First().Value2.ShouldNotBeNull();
            target.First().Value2.ProductId.ShouldBe("654");
            target.First().Value2.Price.ShouldBe(11.99);

            target.Second().ShouldNotBeSameAs(itemOne);
            target.Second().Value1.ShouldBe(123);
            target.Second().Value2.ShouldNotBeNull();
            target.Second().Value2.ProductId.ShouldBe("321");
            target.Second().Value2.Price.ShouldBe(99.99);
        }

        [Fact]
        public void ShouldErrorIfNoCompositeIdentifiersSupplied()
        {
            var idsEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.InstancesOf<Person>().IdentifyUsing();
                }
            });

            idsEx.Message.ShouldContain("composite identifier values must be specified");
            idsEx.InnerException.ShouldBeOfType<ArgumentException>();
        }

        [Fact]
        public void ShouldErrorIfNullCompositeIdentifierSupplied()
        {
            var idsEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.InstancesOf<Person>().IdentifyUsing(p => p.Name, null);
                }
            });

            idsEx.Message.ShouldContain("composite identifier values must be non-null");
            idsEx.InnerException.ShouldBeOfType<ArgumentNullException>();
        }

        [Fact]
        public void ShouldErrorIfRedundantIdentifierSupplied()
        {
            var idEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.InstancesOf<Person>().IdentifyUsing(p => p.Id);
                }
            });

            idEx.Message.ShouldContain("Id is automatically used as the identifier");
            idEx.Message.ShouldContain("does not need to be configured");
        }

        [Fact]
        public void ShouldErrorIfRedundantCustomNamingIdentifierSupplied()
        {
            var idEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping.UseNamePattern("^_(.+)_$");

                    mapper.WhenMapping
                        .InstancesOf(new { _Id_ = default(int) })
                        .IdentifyUsing(d => d._Id_);
                }
            });

            idEx.Message.ShouldContain("_Id_ is automatically used as the identifier");
            idEx.Message.ShouldContain("does not need to be configured");
        }

        [Fact]
        public void ShouldErrorIfMultipleIdentifiersSuppliedForSameType()
        {
            Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .InstancesOf<Person>()
                        .IdentifyUsing(p => p.Name);

                    mapper.WhenMapping
                        .InstancesOf<Person>()
                        .IdentifyUsing(p => p.Title);
                }
            });
        }
    }
}
