namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using AgileMapper.Extensions;
    using Common;
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
        public void ShouldApplyANestedAlternateOverwriteDataSource()
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
        public void ShouldApplyAnAlternateSimpleTypeExpressionResult()
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