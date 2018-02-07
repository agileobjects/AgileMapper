namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Configuration;
    using NetStandardPolyfills;
    using TestClasses;
    using Xunit;

    public class WhenIgnoringMembers
    {
        [Fact]
        public void ShouldIgnoreAConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .Ignore(pvm => pvm.Name);

                var source = new PersonViewModel { Name = "Jon" };
                var result = mapper.Map(source).ToANew<Person>();

                result.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberInARootCollection()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .Over<Person>()
                    .Ignore(p => p.Address);

                var source = new[] { new Person { Name = "Jon", Address = new Address { Line1 = "Blah" } } };
                var target = new[] { new Person() };
                var result = mapper.Map(source).Over(target);

                result.Length.ShouldBe(1);
                result.First().Address.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .If(ctx => ctx.Source.Name == "Bilbo")
                    .Ignore(x => x.Name);

                var matchingSource = new PersonViewModel { Name = "Bilbo" };
                var matchingResult = mapper.Map(matchingSource).ToANew<Person>();

                matchingResult.Name.ShouldBeNull();

                var nonMatchingSource = new PersonViewModel { Name = "Frodo" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<Person>();

                nonMatchingResult.Name.ShouldBe("Frodo");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberForASpecifiedRuleSetConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .OnTo<Address>()
                    .If(ctx => ctx.Source.Name == "Gandalf")
                    .Ignore(a => a.Line1);

                var source = new PersonViewModel { Name = "Gandalf", AddressLine1 = "??" };
                var onToResult = mapper.Map(source).OnTo(new Person { Address = new Address() });

                onToResult.Name.ShouldBe("Gandalf");
                onToResult.Address.Line1.ShouldBeNull();

                var createNewResult = mapper.Map(source).ToANew<Person>();

                createNewResult.Name.ShouldBe("Gandalf");
                createNewResult.Address.Line1.ShouldBe("??");
            }
        }

        [Fact]
        public void ShouldIgnoreMultipleConfiguredMembers()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Bilbo",
                    AddressLine1 = "House Street",
                    AddressLine2 = "Town City"
                };

                mapper.WhenMapping
                    .From(source)
                    .ToANew<Person>()
                    .Ignore(p => p.Name, p => p.Address.Line1);

                var matchingResult = mapper.Map(source).ToANew<Person>();

                matchingResult.Id.ToString().ShouldBe(source.Id);
                matchingResult.Name.ShouldBeNull();
                matchingResult.Address.ShouldNotBeNull();
                matchingResult.Address.Line1.ShouldBeNull();
                matchingResult.Address.Line2.ShouldBe("Town City");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberInACollectionConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .ToANew<Person>()
                    .If(ctx => ctx.Source.Name.StartsWith("F"))
                    .Ignore(p => p.Name);

                var source = new[]
                {
                    new PersonViewModel { Name = "Bilbo" },
                    new PersonViewModel { Name = "Frodo" }
                };

                var result = mapper.Map(source).ToANew<IEnumerable<Person>>();

                result.Count().ShouldBe(2);

                result.First().Name.ShouldBe("Bilbo");
                result.Second().Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredMemberByEnumerableElementConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .ToANew<PublicProperty<string>>()
                    .If(ctx => ctx.EnumerableIndex > 0)
                    .Ignore(p => p.Value);

                var source = new[]
                {
                    new PublicProperty<int> { Value = 123 },
                    new PublicProperty<int> { Value = 456 }
                };

                var result = mapper.Map(source).ToANew<IEnumerable<PublicProperty<string>>>();

                result.Count().ShouldBe(2);

                result.First().Value.ShouldBe("123");
                result.Second().Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAReadOnlyComplexTypeMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicReadOnlyProperty<Address>>()
                    .Ignore(psm => psm.Value);

                var source = new PublicField<Address> { Value = new Address { Line1 = "Use this!" } };
                var target = new PublicReadOnlyProperty<Address>(new Address { Line1 = "Ignore this!" });

                mapper.Map(source).Over(target);

                target.Value.Line1.ShouldBe("Ignore this!");
            }
        }

        [Fact]
        public void ShouldSupportRedundantIgnoreConflictingWithConditionalIgnore()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .If((p, pvm) => p.Name == "Frank")
                    .Ignore(pvm => pvm.Name);

                mapper.WhenMapping
                    .From<Customer>()
                    .To<CustomerViewModel>()
                    .Ignore(cvm => cvm.Name);

                var matchingPersonResult = mapper.Map(new Person { Name = "Frank" }).ToANew<PersonViewModel>();
                var nonMatchingPersonResult = mapper.Map(new Person { Name = "Dennis" }).ToANew<PersonViewModel>();
                var customerResult = mapper.Map(new Customer { Name = "Mac" }).ToANew<CustomerViewModel>();

                matchingPersonResult.Name.ShouldBeNull();
                nonMatchingPersonResult.Name.ShouldBe("Dennis");
                customerResult.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldSupportRedundantConditionalIgnoreConflictingWithIgnore()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(pvm => pvm.Name);

                mapper.WhenMapping
                    .From<Customer>()
                    .To<CustomerViewModel>()
                    .If((c, cvm) => c.Name == "Frank")
                    .Ignore(cvm => cvm.Name);

                var personResult = mapper.Map(new Person { Name = "Dennis" }).ToANew<PersonViewModel>();
                var matchingCustomerResult = mapper.Map(new Customer { Name = "Mac" }).ToANew<CustomerViewModel>();
                var nonMatchingCustomerResult = mapper.Map(new Customer { Name = "Frank" }).ToANew<CustomerViewModel>();

                personResult.Name.ShouldBeNull();
                matchingCustomerResult.Name.ShouldBe("Mac");
                nonMatchingCustomerResult.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldSupportSamePathIgnoredMembersWithDifferentSourceTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .Ignore(x => x.Value);

                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicProperty<int>>()
                    .Ignore(x => x.Value);
            }
        }

        [Fact]
        public void ShouldSupportSamePathIgnoredMembersWithDifferentTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .Ignore(x => x.Name);

                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .To<Person>()
                    .Ignore(x => x.Name);
            }
        }

        [Fact]
        public void ShouldCompareIgnoredMembersConsistently()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.To<PublicField<string>>().Ignore(pf => pf.Value);
                mapper.WhenMapping.To<PublicProperty<string>>().Ignore(pp => pp.Value);

                var configurations = ((IMapperInternal)mapper).Context.UserConfigurations;
                var ignoredMembersProperty = configurations.GetType().GetNonPublicInstanceProperty("IgnoredMembers");
                var ignoredMembersValue = ignoredMembersProperty.GetValue(configurations, Enumerable<object>.EmptyArray);
                var ignoredMembers = (IList<ConfiguredIgnoredMember>)ignoredMembersValue;

                ignoredMembers.Count.ShouldBe(2);

                var ignore1 = (IComparable<UserConfiguredItemBase>)ignoredMembers.First();
                var ignore2 = (IComparable<UserConfiguredItemBase>)ignoredMembers.Second();

                var compareResult1 = ignore1.CompareTo(ignoredMembers.Second());
                var compareResult2 = ignore2.CompareTo(ignoredMembers.First());

                compareResult1.ShouldNotBe(compareResult2);
            }
        }
    }
}