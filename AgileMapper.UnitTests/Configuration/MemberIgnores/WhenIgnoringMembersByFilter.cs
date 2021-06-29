namespace AgileObjects.AgileMapper.UnitTests.Configuration.MemberIgnores
{
    using System;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Common.TestClasses;
    using NetStandardPolyfills;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringMembersByFilter
    {
        [Fact]
        public void ShouldIgnoreMembersByTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicTwoFields<int, long>>()
                    .IgnoreTargetMembersOfType<int>();

                var source = new { Value1 = 1, Value2 = 2 };

                var matchingResult = mapper.Map(source).ToANew<PublicTwoFields<int, long>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe(2L);

                var nonMatchingResult = mapper.Map(source).ToANew<PublicTwoFields<long, int>>();
                nonMatchingResult.Value1.ShouldBe(1L);
                nonMatchingResult.Value2.ShouldBe(2L);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersByTypeSourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .To<PublicField<int>>()
                    .IgnoreTargetMembersOfType<int>();

                var matchingSource = new PublicProperty<string> { Value = "999" };
                var nonMatchingSource = new { Value = 987 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicProperty<int>>();
                nonMatchingTargetResult.Value.ShouldBe(999);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingSourceResult.Value.ShouldBe(987);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<int>>();
                nonMatchingResult.Value.ShouldBe(987);
            }
        }

        [Fact]
        public void ShouldIgnorePropertiesByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicProperty<string>>()
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var nonMatchingResult = mapper.Map(new { Line1 = "xyz" }).ToANew<Address>();
                nonMatchingResult.Line1.ShouldBe("xyz");

                var matchingResult = mapper.Map(new { Value = "xyz" }).ToANew<PublicProperty<string>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnorePropertiesBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .To<PublicProperty<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsProperty);

                var matchingSource = new PublicField<string> { Value = "123" };
                var nonMatchingSource = new { Value = 456 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicProperty<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(123);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<int>>();
                nonMatchingSourceResult.Value.ShouldBe(456);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldIgnorePropertiesBySourceTypeTargetTypeAndPropertyInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingSource = new { Line2 = "Here" };
                var nonMatchingSource = new { Line1 = "Where?", Line2 = "There" };

                mapper.WhenMapping
                    .From(matchingSource)
                    .To<Address>()
                    .IgnoreTargetMembersWhere(member =>
                        member.IsPropertyMatching(p => p.GetSetMethod().Name.EndsWith("Line2")));

                mapper.WhenMapping
                    .From(matchingSource)
                    .To<PublicProperty<string>>()
                    .Map(ctx => ctx.Source.Line2)
                    .To(pp => pp.Value);

                mapper.WhenMapping
                    .From(nonMatchingSource)
                    .To<PublicProperty<string>>()
                    .Map(ctx => ctx.Source.Line2)
                    .To(pp => pp.Value);

                var matchingResult = mapper.Map(matchingSource).ToANew<Address>();
                matchingResult.Line2.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicProperty<string>>();
                nonMatchingTargetResult.Value.ShouldBe("Here");

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<Address>();
                nonMatchingSourceResult.Line1.ShouldBe("Where?");
                nonMatchingSourceResult.Line2.ShouldBe("There");

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<string>>();
                nonMatchingResult.Value.ShouldBe("There");
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicField<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsField);

                var nonMatchingResult = mapper.Map(new { Value = "x" }).ToANew<PublicField<char>>();
                nonMatchingResult.Value.ShouldBe('x');

                var matchingResult = mapper.Map(new { Value = "5" }).ToANew<PublicField<int>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<long>>()
                    .To<PublicField<short>>()
                    .IgnoreTargetMembersWhere(member => member.IsField);

                var matchingSource = new PublicField<long> { Value = 111L };
                var nonMatchingSource = new { Value = 222L };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<short>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(111);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicField<short>>();
                nonMatchingSourceResult.Value.ShouldBe(222);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(222);
            }
        }

        [Fact]
        public void ShouldIgnoreFieldsBySourceTypeTargetTypeAndFieldInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var field2HashCode = typeof(PublicTwoFields<string, string>)
                    .GetPublicInstanceField("Value2")
                    .GetHashCode();

                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<PublicTwoFields<string, string>>()
                    .IgnoreTargetMembersWhere(member =>
                        member.IsFieldMatching(f => f.GetHashCode() == field2HashCode));

                var matchingSource = new PublicTwoFields<int, int> { Value1 = 111, Value2 = 222 };
                var nonMatchingSource = new { Value1 = 333, Value2 = 444 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, string>>();
                matchingResult.Value1.ShouldBe("111");
                matchingResult.Value2.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, int>>();
                nonMatchingTargetResult.Value1.ShouldBe("111");
                nonMatchingTargetResult.Value2.ShouldBe(222);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<string, string>>();
                nonMatchingSourceResult.Value1.ShouldBe("333");
                nonMatchingSourceResult.Value2.ShouldBe("444");

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<int, string>>();
                nonMatchingResult.Value1.ShouldBe(333);
                nonMatchingResult.Value2.ShouldBe("444");
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .To<PublicSetMethod<DateTime>>()
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var nonMatchingResult = mapper.Map(new { Value = 'x' }).ToANew<PublicSetMethod<string>>();
                nonMatchingResult.Value.ShouldBe("x");

                var matchingResult = mapper.Map(new { Value = DateTime.Now }).ToANew<PublicSetMethod<DateTime>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicSetMethod<int>>()
                    .IgnoreTargetMembersWhere(member => member.IsSetMethod);

                var matchingSource = new PublicGetMethod<int>(888);
                var nonMatchingSource = new { Value = 333 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(888);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<int>>();
                nonMatchingSourceResult.Value.ShouldBe(333);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(333);
            }
        }

        [Fact]
        public void ShouldIgnoreSetMethodsBySourceTypeTargetTypeAndMethodInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicSetMethod<int>>()
                    .IgnoreTargetMembersWhere(member =>
                        member.IsSetMethodMatching(m =>
                            m.GetParameters()[0].ParameterType == typeof(int)));

                var matchingSource = new PublicGetMethod<int>(999);
                var nonMatchingSource = new { Value = 111 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<int>>();
                matchingResult.Value.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(999);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<int>>();
                nonMatchingSourceResult.Value.ShouldBe(111);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(111);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersBySourceTypeTargetTypeAndNameMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<PublicTwoFields<int, int>>()
                    .IgnoreTargetMembersWhere(member => member.Name.Contains("Value1"));

                var matchingSource = new PublicTwoFields<int, int> { Value1 = 1, Value2 = 2 };
                var nonMatchingSource = new { Value1 = -1, Value2 = -2 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<int, int>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe(2);

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicTwoParamCtor<int, int>>();
                nonMatchingTargetResult.Value1.ShouldBe(1);
                nonMatchingTargetResult.Value2.ShouldBe(2);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<int, int>>();
                nonMatchingSourceResult.Value1.ShouldBe(-1);
                nonMatchingSourceResult.Value2.ShouldBe(-2);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoParamCtor<int, int>>();
                nonMatchingResult.Value1.ShouldBe(-1);
                nonMatchingResult.Value2.ShouldBe(-2);
            }
        }

        [Fact]
        public void ShouldIgnoreMembersBySourceTypeTargetTypeAndPathMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .To<PublicProperty<Address>>()
                    .IgnoreTargetMembersWhere(member => member.Path.EqualsIgnoreCase("Value.Line2"));

                var matchingSource = new PublicField<Address> { Value = new Address { Line1 = "Here", Line2 = "Here!" } };
                var nonMatchingSource = new { Value = new Address { Line1 = "There", Line2 = "There!" } };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicProperty<Address>>();
                matchingResult.Value.Line1.ShouldBe("Here");
                matchingResult.Value.Line2.ShouldBeDefault();

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<Address>>();
                nonMatchingTargetResult.Value.Line1.ShouldBe("Here");
                nonMatchingTargetResult.Value.Line2.ShouldBe("Here!");

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<Address>>();
                nonMatchingSourceResult.Value.Line1.ShouldBe("There");
                nonMatchingSourceResult.Value.Line2.ShouldBe("There!");

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<Address>>();
                nonMatchingResult.Value.Line1.ShouldBe("There");
                nonMatchingResult.Value.Line2.ShouldBe("There!");
            }
        }

        [Fact]
        public void ShouldIgnoreMembersBySourceTypeTargetTypeAndAttribute()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<AttributeHelper>()
                    .IgnoreTargetMembersWhere(member => member.HasAttribute<IgnoreMeAttribute>());

                var matchingSource = new PublicTwoFields<int, int> { Value1 = 10, Value2 = 20 };
                var matchingResult = mapper.Map(matchingSource).ToANew<AttributeHelper>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe("20");

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, string>>();
                nonMatchingTargetResult.Value1.ShouldBe("10");
                nonMatchingTargetResult.Value2.ShouldBe("20");

                var nonMatchingSource = new { Value1 = "11", Value2 = "21" };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<AttributeHelper>();
                nonMatchingSourceResult.Value1.ShouldBe("11");
                nonMatchingSourceResult.Value2.ShouldBe("21");

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<string, string>>();
                nonMatchingResult.Value1.ShouldBe("11");
                nonMatchingResult.Value2.ShouldBe("21");
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