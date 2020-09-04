namespace AgileObjects.AgileMapper.UnitTests.Configuration
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
    public class WhenIgnoringSourceMembersByFilter
    {
        [Fact]
        public void ShouldIgnoreSourceMembersByTypeAndSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, long>>()
                    .IgnoreSourceMembersOfType<int>();

                var matchingSource = new PublicTwoFields<int, long> { Value1 = 1, Value2 = 2L };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<long, int>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe(2);

                var nonMatchingSource = new PublicTwoFields<long, int> { Value1 = 1L, Value2 = 2 };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<long, int>>();
                nonMatchingResult.Value1.ShouldBe(1L);
                nonMatchingResult.Value2.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMembersByTypeSourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .ToANew<PublicField<int>>()
                    .IgnoreSourceMembersOfType<string>();

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
        public void ShouldIgnoreSourcePropertiesByMemberTypeAndSourceType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<string>>()
                    .IgnoreSourceMembersWhere(member => member.IsProperty);

                var nonMatchingSource = new PublicField<string> { Value = "xyz" };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<string>>();
                nonMatchingResult.Value.ShouldBe("xyz");

                var matchingSource = new PublicProperty<string> { Value = "zyx" };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicField<string>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreSourceFieldsBySourceTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<long>>()
                    .To<PublicField<short>>()
                    .IgnoreSourceMembersWhere(member => member.IsField);

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
        public void ShouldIgnoreSourceFieldsBySourceTypeTargetTypeAndFieldInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var field1 = typeof(PublicTwoFields<int, int>).GetPublicInstanceField("Value1");

                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<PublicTwoFields<string, string>>()
                    .IgnoreSourceMembersWhere(member =>
                        member.IsFieldMatching(f => f == field1));

                var matchingSource = new PublicTwoFields<int, int> { Value1 = 111, Value2 = 222 };
                var nonMatchingSource = new { Value1 = 333, Value2 = 444 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, string>>();
                matchingResult.Value1.ShouldBeNull();
                matchingResult.Value2.ShouldBe("222");

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
        public void ShouldIgnoreGetMethodsByMemberTypeAndTargetType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicGetMethod<DateTime>>()
                    .IgnoreSourceMembersWhere(member => member.IsGetMethod);

                var nonMatchingSource = new PublicProperty<DateTime> { Value = DateTime.Today };
                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<string>>();
                nonMatchingResult.Value.ShouldBe(DateTime.Today);

                var matchingSource = new PublicGetMethod<DateTime>(DateTime.Today);
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<DateTime>>();
                matchingResult.Value.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldIgnoreGetMethodsBySourceTypeTargetTypeAndMethodInfoMatcher()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var value = 1;

                // ReSharper disable once AccessToModifiedClosure
                mapper.WhenMapping
                    .From<PublicGetMethod<int>>()
                    .To<PublicSetMethod<int>>()
                    .IgnoreSourceMembersWhere(member =>
                        member.IsGetMethodMatching(m => value == 1));

                var matchingSource = new PublicGetMethod<int>(999);
                var nonMatchingSource = new { Value = 111 };

                var matchingResult = mapper.Map(matchingSource).ToANew<PublicSetMethod<int>>();
                matchingResult.Value.ShouldBeDefault();

                value = 2;
                var nonMatchingFilterResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingFilterResult.Value.ShouldBe(999);

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicField<int>>();
                nonMatchingTargetResult.Value.ShouldBe(999);

                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicSetMethod<int>>();
                nonMatchingSourceResult.Value.ShouldBe(111);

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicField<int>>();
                nonMatchingResult.Value.ShouldBe(111);
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMembersBySourceTypeTargetTypeAndNameMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .To<PublicTwoFields<int, int>>()
                    .IgnoreSourceMembersWhere(member => member.Name.Contains("Value1"));

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
        public void ShouldIgnoreSourceMembersBySourceTypeAndPathMatch()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Address>>()
                    .IgnoreSourceMembersWhere(member => member.Path.EqualsIgnoreCase("Value.Line2"));

                var matchingSource = new PublicField<Address> { Value = new Address { Line1 = "Here", Line2 = "Here!" } };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicProperty<Address>>();
                matchingResult.Value.Line1.ShouldBe("Here");
                matchingResult.Value.Line2.ShouldBeNull();

                var nonMatchingSource = new { Value = new Address { Line1 = "There", Line2 = "There!" } };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicProperty<Address>>();
                nonMatchingSourceResult.Value.Line1.ShouldBe("There");
                nonMatchingSourceResult.Value.Line2.ShouldBe("There!");
            }
        }

        [Fact]
        public void ShouldIgnoreSourceMembersBySourceTypeTargetTypeAndAttribute()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<AttributeHelper>()
                    .To<PublicTwoFields<int, int>>()
                    .IgnoreSourceMembersWhere(member => member.HasAttribute<IgnoreMeAttribute>());

                var matchingSource = new AttributeHelper { Value1 = 10, Value2 = 20 };
                var matchingResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<int, int>>();
                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldBe("20");

                var nonMatchingTargetResult = mapper.Map(matchingSource).ToANew<PublicTwoFields<string, string>>();
                nonMatchingTargetResult.Value1.ShouldBe("10");
                nonMatchingTargetResult.Value2.ShouldBe("20");

                var nonMatchingSource = new { Value1 = "11", Value2 = "21" };
                var nonMatchingSourceResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<int, int>>();
                nonMatchingSourceResult.Value1.ShouldBe("11");
                nonMatchingSourceResult.Value2.ShouldBe("21");

                var nonMatchingResult = mapper.Map(nonMatchingSource).ToANew<PublicTwoFields<string, string>>();
                nonMatchingResult.Value1.ShouldBe("11");
                nonMatchingResult.Value2.ShouldBe("21");
            }
        }

        #region Helper Classes

        public struct AttributeHelper
        {
            [IgnoreMe]
            public int Value1 { get; set; }

            public int Value2 { get; set; }
        }

        public sealed class IgnoreMeAttribute : Attribute
        {
        }

        #endregion
    }
}
