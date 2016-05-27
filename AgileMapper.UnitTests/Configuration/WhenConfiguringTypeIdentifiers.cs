namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using Api.Configuration;
    using Shouldly;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringTypeIdentifiers
    {
        [Fact]
        public void ShouldUseAConfiguredIdentifier()
        {
            using (var mapper = Mapper.Create())
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
                    new PersonViewModel { Id = Guid.NewGuid(), Name = "Boris" }
                };
                var result = mapper.Map(source).OnTo(target);

                result.HasOne().ShouldBeTrue();
                result.First().AddressLine1.ShouldBe("My House");
            }
        }

        [Fact]
        public void ShouldUseAConfiguredIdentifierForADerivedType()
        {
            using (var mapper = Mapper.Create())
            {
                mapper.WhenMapping
                    .InstancesOf<Person>()
                    .IdentifyUsing(p => p.Name);

                var source = new[]
                {
                    new Customer { Title = Title.Dr, Name = "Hetty" }
                };
                var target = new List<Person> { new Person { Title = Title.Ms, Name = "Hetty" } };
                var result = mapper.Map(source).Over(target);

                result.HasOne().ShouldBeTrue();
                result.First().Title.ShouldBe(Title.Dr);
            }
        }

        [Fact]
        public void ShouldErrorIfMultipleIdentifiersSpecifiedForSameType()
        {
            Assert.Throws<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.Create())
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
