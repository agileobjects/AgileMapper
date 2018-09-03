namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Extensions.Internal;
    using Common;
    using NetStandardPolyfills;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringMembersByGlobalFilter
    {
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
        public void ShouldIgnorePropertiesByPropertyInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member =>
                        member.IsPropertyMatching(p => p.GetSetMethod().Name.EndsWith("Line2")));

                var source = new { Line1 = "hfjdk", Line2 = "kejkwen" };

                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("hfjdk");
                result.Line2.ShouldBeNull();
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
        public void ShouldIgnoreFieldsByFieldInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var field1HashCode = typeof(PublicTwoFields<string, string>)
                    .GetPublicInstanceField("Value1")
                    .GetHashCode();

                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member =>
                        member.IsFieldMatching(f => f.GetHashCode() == field1HashCode));

                var source = new { Value1 = "koewlkswmj", Value2 = "lkgflkmdelk" };

                var result = mapper.Map(source).ToANew<PublicTwoFields<string, string>>();

                result.Value1.ShouldBeNull();
                result.Value2.ShouldBe("lkgflkmdelk");
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
        public void ShouldIgnoreSetMethodsByMethodInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .IgnoreTargetMembersWhere(member =>
                        member.IsSetMethodMatching(m =>
                            m.GetParameters().First().ParameterType == typeof(int)));

                var source = new { Value = "673473" };

                var intResult = mapper.Map(source).ToANew<PublicSetMethod<int>>();

                intResult.Value.ShouldBeDefault();

                var longResult = mapper.Map(source).ToANew<PublicSetMethod<long>>();

                longResult.Value.ShouldBe(673473L);
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
                    .IgnoreTargetMembersWhere(member => member.Path.EqualsIgnoreCase("Value.Line1"));

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