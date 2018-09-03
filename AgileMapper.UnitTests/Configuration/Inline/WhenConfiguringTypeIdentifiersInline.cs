namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    }
}
