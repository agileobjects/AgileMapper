namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringToTargetInsteadDataSources
    {
        [Fact]
        public void ShouldUseAnAlternateRootSourceObject()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var source = new { Value1 = 123, Value = new { Value2 = 456 } };

                mapper.WhenMapping
                    .From(source)
                    .To<PublicTwoFields<int, int>>()
                    .Map(ctx => ctx.Source.Value)
                    .ToTargetInstead();

                var result = source
                    .MapUsing(mapper)
                    .ToANew<PublicTwoFields<int, int>>();

                result.Value1.ShouldBeDefault();
                result.Value2.ShouldBe(456);
            }
        }

        [Fact]
        public void ShouldUseANestedAlternateOverwriteDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<int>>>()
                    .Over<PublicField<int>>()
                    .Map((s, t) => s.Value)
                    .ToTargetInstead();

                var source = new PublicTwoFields<int, PublicField<PublicField<int>>>
                {
                    Value1 = 6372,
                    Value2 = new PublicField<PublicField<int>>
                    {
                        Value = new PublicField<int>
                        {
                            Value = 8262
                        }
                    }
                };

                var target = new PublicTwoFields<int, PublicField<int>>
                {
                    Value1 = 637,
                    Value2 = new PublicField<int> { Value = 728 }
                };

                mapper.Map(source).Over(target);

                target.Value1.ShouldBe(6372);
                target.Value2.ShouldNotBeNull().Value.ShouldBe(8262);
            }
        }

        [Fact]
        public void ShouldUseAnAlternateSimpleTypeExpressionResult()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<string>().ToANew<string>()
                    .Map((s, t) => string.IsNullOrEmpty(s) ? null : s)
                    .ToTargetInstead();

                var source = new Address { Line1 = "Here", Line2 = string.Empty };
                var result = mapper.Map(source).ToANew<Address>();

                result.Line1.ShouldBe("Here");
                result.Line2.ShouldBeNull();
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/173
        [Fact]
        public void ShouldUseANestedDictionaryAlternateDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .FromDictionariesWithValueType<Address>()
                    .OnTo<List<Address>>()
                    .Map(ctx => ctx.Source.Values)
                    .ToTargetInstead();

                var source = new PublicProperty<Dictionary<string, Address>>
                {
                    Value = new Dictionary<string, Address>
                    {
                        ["One"] = new Address { Line1 = "1 Street" },
                        ["Two"] = new Address { Line1 = "2 Street" }
                    }
                };

                var target = new PublicField<List<Address>>();

                mapper.Map(source).OnTo(target);

                target.Value.ShouldNotBeNull().Count.ShouldBe(2);
                target.Value.First().Line1.ShouldBe("1 Street");
                target.Value.Second().Line1.ShouldBe("2 Street");
            }
        }

        [Fact]
        public void ShouldUseANestedAlternateDataSourceConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<int>>>()
                    .ToANew<PublicField<int>>()
                    .If(ctx => ctx.Source.Value.Value > 100)
                    .Map((s, t) => s.Value)
                    .ToTargetInstead();

                var matchingSource = new PublicTwoFields<int, PublicField<PublicField<int>>>
                {
                    Value2 = new PublicField<PublicField<int>>
                    {
                        Value = new PublicField<int>
                        {
                            Value = 200
                        }
                    }
                };

                var matchingResult = mapper
                    .Map(matchingSource)
                    .ToANew<PublicTwoFields<int, PublicField<int>>>();

                matchingResult.Value1.ShouldBeDefault();
                matchingResult.Value2.ShouldNotBeNull().Value.ShouldBe(200);

                var nonMatchingSource = new PublicTwoFields<int, PublicField<PublicField<int>>>
                {
                    Value2 = new PublicField<PublicField<int>>
                    {
                        Value = new PublicField<int>
                        {
                            Value = 100
                        }
                    }
                };

                var nonMatchingResult = mapper
                    .Map(nonMatchingSource)
                    .ToANew<PublicTwoFields<int, PublicField<int>>>();

                nonMatchingResult.Value1.ShouldBeDefault();
                nonMatchingResult.Value2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldUseAnAlternateDataSourceForEnumerableElementsConditionally()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, PublicField<int>>>()
                    .To<PublicProperty<string>>()
                    .If((ptf, pp, i) => i > 0 && i % 2 == 0)
                    .Map((ptf, pp, i) => new PublicField<int>
                    {
                        Value = ptf.Value2.Value * 2
                    })
                    .ToTargetInstead();

                var source = new PublicProperty<IList<PublicTwoFields<int, PublicField<int>>>>
                {
                    Value = new[] { 0, 1, 2, 3, 4 }
                        .Select(i => new PublicTwoFields<int, PublicField<int>>
                        {
                            Value1 = i,
                            Value2 = new PublicField<int> { Value = i }
                        })
                        .ToArray()
                };

                var result = mapper
                    .Map(source)
                    .ToANew<PublicField<PublicProperty<string>[]>>();

                var values = result.ShouldNotBeNull().Value.ShouldNotBeEmpty().ToArray();

                values[0].ShouldBeNull();      // Because  i == 0
                values[1].ShouldBeNull();      // Because (i == 1) % 2 == 1
                values[2].Value.ShouldBe("4"); // Because (i == 2) % 2 == 0 and Value == 2
                values[3].ShouldBeNull();      // Because (i == 3) % 2 == 1
                values[4].Value.ShouldBe("8"); // Because (i == 4) % 2 == 0 and Value == 4
            }
        }

        [Fact]
        public void ShouldHandleANullSourceMemberInAnAlternateDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<PublicField<int>>>()
                    .To<PublicField<int>>()
                    .Map((s, t) => s.Value)
                    .ToTargetInstead();

                var source = new PublicTwoFields<int, PublicField<PublicField<int>>>
                {
                    Value1 = 911,
                    Value2 = null
                };

                var result = mapper.Map(source).ToANew<PublicTwoFields<int, PublicField<int>>>();

                result.Value1.ShouldBe(911);
                result.Value2.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldHandleAnExceptionInARootConfiguredAlternateDataSource()
        {
            var mappingEx = Should.Throw<MappingException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    Func<PublicField<PublicField<int>>, PublicField<long>, PublicField<long>> mapValue =
                        (src, tgt) => throw new NotSupportedException("ASPLODE");

                    mapper.WhenMapping
                        .From<PublicField<PublicField<int>>>()
                        .To<PublicField<long>>()
                        .Map(mapValue)
                        .ToTargetInstead();

                    var source = new PublicField<PublicField<int>>();

                    mapper.Map(source).ToANew<PublicField<long>>();
                }
            });

            mappingEx.Message.ShouldContain("PublicField<PublicField<int>> -> PublicField<long>");
            mappingEx.InnerException.ShouldNotBeNull().Message.ShouldBe("ASPLODE");
        }
    }
}