namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using System;
    using AgileMapper.Extensions.Internal;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringStructDataSources
    {
        [Fact]
        public void ShouldApplyAConstantOnCreateNew()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicPropertyStruct<string>>()
                    .To<PublicPropertyStruct<string>>()
                    .Map("-- CONFIGURED --")
                    .To(pps => pps.Value);

                var source = new PublicPropertyStruct<string> { Value = "Mapped!" };
                var result = mapper.Map(source).ToANew<PublicPropertyStruct<string>>();

                result.Value.ShouldBe("-- CONFIGURED --");
            }
        }

        [Fact]
        public void ShouldApplyAConstantOnNestedOverwrite()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<string>>()
                    .Over<PublicPropertyStruct<int>>()
                    .Map("123")
                    .To(pps => pps.Value);

                var source = new PublicField<PublicField<string>>
                {
                    Value = new PublicField<string> { Value = "456" }
                };
                var target = new PublicField<PublicPropertyStruct<int>>
                {
                    Value = new PublicPropertyStruct<int> { Value = 789 }
                };
                var result = mapper.Map(source).Over(target);

                result.Value.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldApplyAConstantByConstructorParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Guid>>()
                    .To<PublicCtorStruct<string>>()
                    .Map("Not a Guid")
                    .ToCtor<string>();

                var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicCtorStruct<string>>();

                result.Value.ShouldBe("Not a Guid");
            }
        }

        [Fact]
        public void ShouldApplyASourceMemberOnCreateNew()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .To<PublicTwoFieldsStruct<Guid, string>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(pfs => pfs.Value1)
                    .And
                    .Map(ctx => ctx.Source.Name)
                    .To(pfs => pfs.Value2);

                var source = new MysteryCustomer { Id = Guid.NewGuid(), Name = "Gyles" };
                var result = mapper.Map(source).ToANew<PublicTwoFieldsStruct<Guid, string>>();

                result.Value1.ShouldBe(source.Id);
                result.Value2.ShouldBe("Gyles");
            }
        }

        [Fact]
        public void ShouldApplyASourceMemberOnMerge()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .OnTo<PublicPropertyStruct<string>>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(pps => pps.Value);

                var source = new MysteryCustomer { Name = "Andy", Address = new Address { Line1 = "Line 1!" } };
                var target = new PublicPropertyStruct<string>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBe("Line 1!");
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionOnOverwrite()
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

        [Fact]
        public void ShouldApplyAnExpressionByConstructorParameterName()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicPropertyStruct<int>>()
                    .To<PublicCtorStruct<long>>()
                    .Map((s, t, i) => s.Value * i)
                    .ToCtor("value");

                var source = new[]
                {
                    new PublicPropertyStruct<int> { Value = 11 },
                    new PublicPropertyStruct<int> { Value = 22 },
                    new PublicPropertyStruct<int> { Value = 33 }
                };
                var result = mapper.Map(source).ToANew<PublicCtorStruct<long>[]>();

                result.Length.ShouldBe(3);

                result.First().Value.ShouldBe(11 * 0);
                result.Second().Value.ShouldBe(22 * 1);
                result.Third().Value.ShouldBe(33 * 2);
            }
        }

        [Fact]
        public void ShouldApplyAnExpressionByConstructorParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Guid>>()
                    .To<PublicCtorStruct<int>>()
                    .Map(ctx => ctx.Source.Value.ToString().Length)
                    .ToCtor<int>();

                var guid = Guid.NewGuid();
                var source = new PublicField<Guid> { Value = guid };
                var result = mapper.Map(source).ToANew<PublicCtorStruct<int>>();

                result.Value.ShouldBe(guid.ToString().Length);
            }
        }

        [Fact]
        public void ShouldHandleANullSourceMemberOnMerge()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<MysteryCustomer>()
                    .OnTo<PublicPropertyStruct<string>>()
                    .Map(ctx => ctx.Source.Address.Line1)
                    .To(pps => pps.Value);

                var source = new MysteryCustomer { Name = "Andy" };
                var target = new PublicPropertyStruct<string>();
                var result = mapper.Map(source).OnTo(target);

                result.Value.ShouldBeNull();
            }
        }
    }
}
