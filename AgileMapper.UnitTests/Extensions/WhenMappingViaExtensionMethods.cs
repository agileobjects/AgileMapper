namespace AgileObjects.AgileMapper.UnitTests.Extensions
{
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

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
                var result = source.Map(_ => _.Using(mapper)).ToANew<PublicCtorStruct<string>>();

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
                var result = source.DeepClone(_ => _.Using(mapper));

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
    }
}
