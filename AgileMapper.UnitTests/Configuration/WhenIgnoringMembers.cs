namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Shouldly;
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
                    .Ignore(x => x.Name);

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
        public void ShouldIgnoreMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersOfType<long>();

                var source = new { Value1 = 1, Value2 = 2 };
                var result = mapper.Map(source).ToANew<PublicTwoFields<int, long>>();

                result.Value1.ShouldBe(1);
                result.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreComplexTypeMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersOfType<Address>();

                var source = new { Value = new { Line1 = "Nope" } };
                var result = mapper.Map(source).ToANew<PublicField<Address>>();

                result.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreDerivedComplexTypeMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersOfType<PersonViewModel>();

                var source = new { value1 = 123, Value2 = new { Name = "Larry" } };
                var result = mapper.Map(source).ToANew<PublicTwoFields<int, CustomerViewModel>>();

                result.Value1.ShouldBe(123);
                result.Value2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodMembersByMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var source = new { Value = 123 };
                var result = mapper.Map(source).ToANew<PublicSetMethod<int>>();

                result.Value.ShouldBeDefault();
            }
        }
    }
}