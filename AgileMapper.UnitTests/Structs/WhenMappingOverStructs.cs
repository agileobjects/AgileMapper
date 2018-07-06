﻿namespace AgileObjects.AgileMapper.UnitTests.Structs
{
    using System;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOverStructs
    {
        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Value1 = Guid.NewGuid(), Value2 = "Mr Pants" };
            var target = new PublicTwoFieldsStruct<Guid, string>()
            {
                Value1 = Guid.NewGuid(),
                Value2 = "Mrs Trousers"
            };
            var result = Mapper.Map(source).Over(target);

            result.ShouldNotBe(target);
            result.Value1.ShouldBe(source.Value1);
            result.Value2.ShouldBe(source.Value2);
        }

        [Fact]
        public void ShouldSetAnExistingSimpleTypePropertyValueToDefault()
        {
            var source = new PublicTwoFields<double?, int>();
            var target = new PublicTwoFieldsStruct<double?, int> { Value1 = 537.0, Value2 = 6382 };

            var result = Mapper.Map(source).Over(target);

            target.Value1.ShouldBe(537.0m);
            target.Value2.ShouldBe(6382);

            result.Value1.ShouldBeNull();
            result.Value2.ShouldBeDefault();
        }

        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var target = new PublicPropertyStruct<Guid>();
            var result = Mapper.Map(default(PublicField<string>)).Over(target);

            result.ShouldBe(target);
        }

        [Fact]
        public void ShouldApplyAConfiguredExpression()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .Over<PublicTwoFieldsStruct<int, int>>()
                    .Map(ctx => ctx.Source.Value)
                    .To(pps => pps.Value1)
                    .And
                    .Map((pf, ptfs, i) => pf.Value + ptfs.Value2)
                    .To(pps => pps.Value2);

                var source = new PublicField<int> { Value = 63872 };
                var target = new PublicTwoFieldsStruct<int, int> { Value1 = 1, Value2 = 2 };
                var result = mapper.Map(source).Over(target);

                result.Value1.ShouldBe(63872);
                result.Value2.ShouldBe(63874);
            }
        }
    }
}