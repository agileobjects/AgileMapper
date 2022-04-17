namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using System;
    using System.Globalization;
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
    [Trait("Category", "Checked")]
    public class WhenMappingViaExtensionMethods
    {
        [Fact]
        public void ShouldCreateNewWithAConvertedIntValue()
        {
            var source = new PublicProperty<int> { Value = 123 };
            var result = source.Map().ToANew<PublicCtorStruct<string>>();

            result.Value.ShouldBe("123");
        }

        [Fact]
        public void ShouldCreateNewWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .To<PublicCtorStruct<string>>()
                    .Map(ctx => ctx.Source.Value * 2)
                    .ToCtor<string>();

                var source = new PublicField<int> { Value = 20 };
                var result = source.MapUsing(mapper).ToANew<PublicCtorStruct<string>>();

                result.Value.ShouldBe("40");
            }
        }

        [Fact]
        public void ShouldCreateNewWithInlineConfiguration()
        {
            var source = new PublicField<int> { Value = 20 };

            var result = source.Map().ToANew<PublicCtorStruct<string>>(cfg => cfg
                .Map(ctx => ctx.Source.Value * 3)
                .ToCtor<string>());

            result.Value.ShouldBe("60");
        }

        [Fact]
        public void ShouldDeepCloneWithACopiedIntValue()
        {
            var source = new PublicProperty<int> { Value = 123 };
            var result = source.DeepClone();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldDeepCloneWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoFieldsStruct<int, int>>()
                    .Map(ctx => ctx.Source.Value1)
                    .To(p => p.Value2)
                    .And
                    .Map(ctx => ctx.Source.Value2)
                    .To(p => p.Value1);

                var source = new PublicTwoFieldsStruct<int, int> { Value1 = 123, Value2 = 456 };
                var result = source.DeepCloneUsing(mapper);

                result.ShouldNotBe(source);
                result.Value1.ShouldBe(456);
                result.Value2.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldDeepCloneWithInlineConfiguration()
        {
            var source = new PublicTwoFieldsStruct<int, int> { Value1 = 456, Value2 = 123 };

            var result = source.DeepClone(cfg => cfg
                .Map(ctx => ctx.Source.Value1)
                .To(p => p.Value2)
                .And
                .Map(ctx => ctx.Source.Value2)
                .To(p => p.Value1));

            result.ShouldNotBe(source);
            result.Value1.ShouldBe(123);
            result.Value2.ShouldBe(456);
        }

        [Fact]
        public void ShouldMergeAParsedStringValue()
        {
            var source = new PublicTwoFields<string, string>
            {
                Value1 = "123",
                Value2 = "456"
            };

            var target = new PublicTwoFieldsStruct<long, long>
            {
                Value1 = 678L
            };

            var result = source.Map().OnTo(target);

            result.Value1.ShouldBe(678L);
            result.Value2.ShouldBe(456L);
        }

        [Fact]
        public void ShouldMergeWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .OnTo<PublicField<string>>()
                    .Map(ctx => ctx.Source.Value / 2)
                    .To(pf => pf.Value);

                var source = new PublicField<int> { Value = 20 };
                var target = new PublicField<string>();

                source.MapUsing(mapper).OnTo(target);

                target.Value.ShouldBe("10");
            }
        }

        [Fact]
        public void ShouldMergeWithInlineConfiguration()
        {
            var source = new PublicTwoFieldsStruct<int, int> { Value1 = 456, Value2 = 123 };
            var target = new PublicTwoFields<int, int> { Value2 = 890 };

            source.Map().OnTo(target, cfg => cfg
                .Map(ctx => ctx.Source.Value1)
                .To(p => p.Value2)
                .And
                .Map(ctx => ctx.Source.Value2)
                .To(p => p.Value1));

            target.Value1.ShouldBe(123);
            target.Value2.ShouldBe(890);
        }

        [Fact]
        public void ShouldOverwriteAParsedStringValue()
        {
            var source = new PublicField<string>
            {
                Value = DateTime.Today.ToString(CultureInfo.CurrentCulture.DateTimeFormat)
            };

            var target = new PublicField<DateTime>
            {
                Value = DateTime.Today.AddDays(-1)
            };

            source.Map().Over(target);

            target.Value.ShouldBe(DateTime.Today);
        }

        [Fact]
        public void ShouldOverwriteWithASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .Over<PublicProperty<string>>()
                    .Map(ctx => ctx.Source.Value + 10)
                    .To(pf => pf.Value);

                var source = new PublicField<int> { Value = 20 };
                var target = new PublicProperty<string>();

                source.MapUsing(mapper).Over(target);

                target.Value.ShouldBe("30");
            }
        }

        [Fact]
        public void ShouldOverwriteWithInlineConfiguration()
        {
            var source = new PublicTwoFieldsStruct<int, int> { Value1 = 456, Value2 = 123 };
            var target = new PublicTwoFields<int, int> { Value1 = 627, Value2 = 890 };

            source.Map().Over(target, cfg => cfg
                .Map(ctx => ctx.Source.Value1)
                .To(p => p.Value2)
                .And
                .Map(ctx => ctx.Source.Value2)
                .To(p => p.Value1));

            target.Value1.ShouldBe(123);
            target.Value2.ShouldBe(456);
        }

        [Fact]
        public void ShouldOverwriteWithInlineConfigurationAndASpecifiedMapper()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<int, int>>()
                    .Over<PublicTwoFields<string, string>>()
                    .Map(ctx => ctx.Source.Value1 + 10)
                    .To(pf => pf.Value1);

                var source = new PublicTwoFields<int, int> { Value1 = 20, Value2 = 20 };
                var target = new PublicTwoFields<string, string>();

                source.MapUsing(mapper).Over(target, cfg => cfg
                    .Map((s, t) => s.Value2 + 5)
                    .To(ptf => ptf.Value2));

                target.Value1.ShouldBe("30");
                target.Value2.ShouldBe("25");
            }
        }
    }
}
