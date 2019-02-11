namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringTypeIdentifiersInline
    {
        [Fact]
        public void ShouldUseAnIdentifierConfiguredInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new[]
                {
                    new Person { Id = Guid.NewGuid(), Name = "Boris", Address = new Address { Line1 = "My House" } }
                };

                var target = new List<PersonViewModel>
                {
                    new PersonViewModel { Id = Guid.NewGuid(), Name = "Boris" }
                };

                mapper
                    .Map(source)
                    .OnTo(
                        target,
                        cfg => cfg.WhenMapping.InstancesOf<Person>().IdentifyUsing(p => p.Name),
                        cfg => cfg.WhenMapping.InstancesOf<PersonViewModel>().IdentifyUsing(p => p.Name));

                target.ShouldHaveSingleItem();
                target.First().AddressLine1.ShouldBe("My House");
            }
        }

        [Fact]
        public void ShouldExtendConfiguredIdentifiersInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.InstancesOf<Person>().IdentifyUsing(p => p.Name);

                var source = new[]
                {
                    new Person { Id = Guid.NewGuid(), Name = "Teresa", Address = new Address { Line1 = "My House" } }
                };

                var target = new List<PersonViewModel>
                {
                    new PersonViewModel { Id = Guid.NewGuid(), Name = "Teresa" }
                };

                mapper
                    .Map(source)
                    .OnTo(target, cfg => cfg
                        .WhenMapping
                        .InstancesOf<PersonViewModel>().IdentifyUsing(p => p.Name));

                target.ShouldHaveSingleItem();
                target.First().AddressLine1.ShouldBe("My House");

                target.First().AddressLine1 = null;

                mapper
                    .Map(source)
                    .OnTo(target, cfg => cfg
                        .WhenMapping
                        .InstancesOf<PersonViewModel>().IdentifyUsing(p => p.Name));

                target.ShouldHaveSingleItem();
                target.First().AddressLine1.ShouldBe("My House");

                mapper.InlineContexts().ShouldHaveSingleItem();
            }
        }

        [Fact]
        public void ShouldUseACompositeIdentifierInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new[]
                {
                    new WeddingDto
                    {
                        BrideName = "Nat",
                        GroomName = "Andy",
                        BrideAddressLine1 = "Nat + Andy's House",
                        GroomAddressLine1 = "Nat + Andy's House"
                    },
                    new WeddingDto
                    {
                        BrideName = "Timea",
                        GroomName = "David",
                        BrideAddressLine1 = "Timea + David's House",
                        GroomAddressLine1 = "Timea + David's House"
                    }
                };

                var target = new List<WeddingDto>
                {
                    new WeddingDto
                    {
                        BrideName = "Nat",
                        GroomName = "Andy"
                    },
                    new WeddingDto
                    {
                        BrideName = "Kate",
                        GroomName = "Steve"
                    }
                };

                mapper.Map(source).OnTo(target, cfg => cfg
                    .WhenMapping
                    .InstancesOf<WeddingDto>()
                    .IdentifyUsing(a => a.BrideName, a => a.GroomName));

                target.Count.ShouldBe(3);

                target.First().BrideName.ShouldBe("Nat");
                target.First().GroomName.ShouldBe("Andy");
                target.First().BrideAddressLine1.ShouldBe("Nat + Andy's House");
                target.First().GroomAddressLine1.ShouldBe("Nat + Andy's House");

                target.Second().BrideName.ShouldBe("Kate");
                target.Second().GroomName.ShouldBe("Steve");
                target.Second().BrideAddressLine1.ShouldBeNull();
                target.Second().GroomAddressLine1.ShouldBeNull();

                target.Third().BrideName.ShouldBe("Timea");
                target.Third().GroomName.ShouldBe("David");
                target.Third().BrideAddressLine1.ShouldBe("Timea + David's House");
                target.Third().GroomAddressLine1.ShouldBe("Timea + David's House");
            }
        }

        [Fact]
        public void ShouldErrorIfUnidentifiableComplexTypeIdentifierSupplied()
        {
            var idsEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    var source = new[]
                    {
                        new PublicTwoFields<int, PublicField<int>>()
                    };

                    mapper.Map(source).Over(new List<PublicTwoFields<int, PublicField<int>>>(1), cfg => cfg
                        .WhenMapping
                        .InstancesOf<PublicTwoFields<int, PublicField<int>>>()
                        .IdentifyUsing(ptf => ptf.Value1, ptf => ptf.Value2));
                }
            });

            idsEx.Message.ShouldContain("Unable to determine identifier");
            idsEx.Message.ShouldContain("ptf.Value2 of Type 'PublicField<int>'");
            idsEx.InnerException.ShouldBeOfType<ArgumentException>();
        }
    }
}
