namespace AgileObjects.AgileMapper.UnitTests.Structs.Configuration
{
    using System;
    using AgileMapper.Extensions;
    using TestClasses;
    using Xunit;

    public class WhenConfiguringStructDataSources
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantByConstructorParameterType()
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
        public void ShouldApplyAConfiguredExpressionByConstructorParameterType()
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
        public void ShouldApplyAConfiguredExpressionByConstructorParameterName()
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
        public void ShouldApplyAConfiguredConstant()
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
        public void ShouldApplyAConfiguredMember()
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
    }
}
