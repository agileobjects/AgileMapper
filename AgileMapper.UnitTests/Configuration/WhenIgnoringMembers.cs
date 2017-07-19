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
        public void ShouldIgnoreObjectMembersByType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersOfType<object>();

                var source = new { Value1 = "object?!", Value2 = new { Line1 = "Address!" } };
                var result = mapper.Map(source).ToANew<PublicTwoFields<object, Address>>();

                result.Value1.ShouldBeNull();
                result.Value2.ShouldNotBeNull();
                result.Value2.Line1.ShouldBe("Address!");
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
        public void ShouldIgnorePropertiesByMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var source = new { Value = "Don't ignore me!" };
                var propertyResult = mapper.Map(source).ToANew<PublicProperty<string>>();
                var setMethodResult = mapper.Map(source).ToANew<PublicSetMethod<string>>();

                propertyResult.Value.ShouldBeDefault();
                setMethodResult.Value.ShouldBe("Don't ignore me!");
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsByMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.IsField);

                var source = new { Value = 873982 };
                var setMethodResult = mapper.Map(source).ToANew<PublicSetMethod<long>>();
                var fieldResult = mapper.Map(source).ToANew<PublicField<long>>();

                fieldResult.Value.ShouldBeDefault();
                setMethodResult.Value.ShouldBe(873982);
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsByMemberType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var source = new { Value = 123 };
                var setMethodResult = mapper.Map(source).ToANew<PublicSetMethod<int>>();
                var fieldResult = mapper.Map(source).ToANew<PublicField<int>>();

                setMethodResult.Value.ShouldBeDefault();
                fieldResult.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersByNameMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.Name.Contains("Value2"));

                var source = new { Value1 = "One!", Value2 = "Two!" };
                var result = mapper.Map(source).ToANew<PublicTwoFields<string, string>>();

                result.Value1.ShouldBe("One!");
                result.Value2.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreMembersByPathMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Address = new Address { Line1 = "ONE!", Line2 = "TWO!" } };

                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member =>
                        member.Path.Equals("Value.Line1", StringComparison.OrdinalIgnoreCase));

                mapper.WhenMapping
                    .From(source)
                    .To<PublicField<Address>>()
                    .Map((s, pf) => s.Address)
                    .To(pf => pf.Value);

                var result = mapper.Map(source).ToANew<PublicField<Address>>();

                result.Value.Line1.ShouldBeDefault();
                result.Value.Line2.ShouldBe("TWO!");
            }
        }

        [Fact]
        public void ShouldIgnoreMembersByAttribute()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.HasAttribute<IgnoreMeAttribute>());

                var source = new { Value1 = "hgfd", Value2 = default(string) };

                var result = mapper.Map(source).ToANew<AttributeHelper>();

                result.Value1.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldNotAttemptToIgnoreAttributedConstructorParameters()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member => member.HasAttribute<IgnoreMeAttribute>());

                var source = new { Value2 = "hjkhgff" };

                var result = mapper.Map(source).ToANew<AttributeHelper>();

                result.Value1.ShouldBeNull();
                result.Value2.ShouldBe("hjkhgff");
            }
        }

        [Fact]
        public void ShouldSupportOverlappingIgnoreFilters()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersOfType<Address>()
                    .AndWhenMapping
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var source = new { Value = new Address { Line1 = "ONE!", Line2 = "TWO!" } };

                var result = mapper.Map(source).ToANew<PublicProperty<Address>>();

                result.Value.ShouldBeNull();
            }
        }

        #region Helper Classes

        public class AttributeHelper
        {
            public AttributeHelper([IgnoreMe]string value2)
            {
                Value2 = value2;
            }

            [IgnoreMe]
            public string Value1 { get; set; }

            public string Value2 { get; }
        }

        public sealed class IgnoreMeAttribute : Attribute
        {
        }

        #endregion
    }
}