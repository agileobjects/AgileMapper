namespace AgileObjects.AgileMapper.UnitTests.Configuration.MemberIgnores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembers
    {
        [Fact]
        public void ShouldIgnoreAConfiguredSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<IdTesterSource>()
                    .ToANew<IdTesterTarget>()
                    .IgnoreSource(id => id.Id);

                var source = new IdTesterSource { Id = "Id!", Identifier = "Identifier!" };
                var result = mapper.Map(source).ToANew<IdTesterTarget>();

                result.Id.ShouldBe("Identifier!");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredSourceMemberConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<PublicField<string>>()
                    .If(ctx => ctx.Source.Value < 5)
                    .IgnoreSource(pf => pf.Value);

                var matchingSource = new PublicField<int> { Value = 3 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<string>>();

                matchingResult.Value.ShouldBeNull();

                var nonMatchingSource = new PublicField<int> { Value = 7 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<string>>();

                nonMatchingResult.Value.ShouldBe("7");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredSourceMemberForASpecifiedRuleSetConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PersonViewModel>()
                    .Over<Address>()
                    .If(ctx => ctx.Source.Name == "Gandalf")
                    .IgnoreSource(pvm => pvm.AddressLine1);

                var source = new PersonViewModel { Name = "Gandalf", AddressLine1 = "??" };
                var overResult = mapper.Map(source).Over(new Person { Address = new Address() });

                overResult.Name.ShouldBe("Gandalf");
                overResult.Address.Line1.ShouldBeNull();

                var createNewResult = mapper.Map(source).ToANew<Person>();

                createNewResult.Name.ShouldBe("Gandalf");
                createNewResult.Address.Line1.ShouldBe("??");
            }
        }

        [Fact]
        public void ShouldIgnoreMultipleConfiguredSourceMembers()
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
                    .IgnoreSource(d => d.Name, d => d.AddressLine1);

                var matchingResult = mapper.Map(source).ToANew<Person>();

                matchingResult.Id.ToString().ShouldBe(source.Id);
                matchingResult.Name.ShouldBeNull();
                matchingResult.Address.ShouldNotBeNull();
                matchingResult.Address.Line1.ShouldBeNull();
                matchingResult.Address.Line2.ShouldBe("Town City");
            }
        }

        [Fact]
        public void ShouldIgnoreAConfiguredSourceMemberInACollectionConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CustomerViewModel>()
                    .ToANew<Customer>()
                    .If((cvm, c) => cvm.Name.StartsWith("F"))
                    .IgnoreSource(cvm => cvm.Name);

                var source = new[]
                {
                    new CustomerViewModel { Name = "Bilbo" },
                    new CustomerViewModel { Name = "Frodo" }
                };

                var result = mapper.Map(source).ToANew<IEnumerable<Customer>>();

                result.Count().ShouldBe(2);

                result.First().Name.ShouldBe("Bilbo");
                result.Second().Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreAComplexTypeSourceMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .IgnoreSource(pfa => pfa.Value);

                var source = new PublicField<Address> { Value = new Address { Line1 = "Use this!" } };
                var target = new PublicReadOnlyProperty<Address>(new Address { Line1 = "Ignore this!" });

                mapper.Map(source).Over(target);

                target.Value.Line1.ShouldBe("Ignore this!");
            }
        }

        [Fact]
        public void ShouldSupportRedundantSourceIgnoreConflictingWithConditionalSourceIgnore()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PersonViewModel>()
                    .If((p, pvm) => p.Name == "Frank")
                    .IgnoreSource(p => p.Name);

                mapper.WhenMapping
                    .From<Customer>()
                    .To<CustomerViewModel>()
                    .IgnoreSource(c => c.Name);

                var matchingPersonResult = mapper.Map(new Person { Name = "Frank" }).ToANew<PersonViewModel>();
                matchingPersonResult.Name.ShouldBeNull();

                var nonMatchingPersonResult = mapper.Map(new Person { Name = "Dennis" }).ToANew<PersonViewModel>();
                nonMatchingPersonResult.Name.ShouldBe("Dennis");

                var customerResult = mapper.Map(new Customer { Name = "Mac" }).ToANew<CustomerViewModel>();
                customerResult.Name.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldSupportSamePathIgnoredSourceMembersWithDifferentTargetTypes()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicProperty<int>>()
                    .IgnoreSource(pp => pp.Value);

                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicField<int>>()
                    .IgnoreSource(pp => pp.Value);
            }
        }

        #region Helper Classes

        private class IdTesterSource
        {
            public string Id { get; set; }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Identifier { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class IdTesterTarget
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string Id { get; set; }
        }

        #endregion
    }
}
