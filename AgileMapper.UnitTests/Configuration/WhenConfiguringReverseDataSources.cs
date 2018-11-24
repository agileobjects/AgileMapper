namespace AgileObjects.AgileMapper.UnitTests.Configuration
{
    using System;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringReverseDataSources
    {
        [Fact]
        public void ShouldReverseAConfiguredMemberByGlobalScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .AutoReverseConfiguredDataSources()
                    .AndWhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(pp => pp.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBe(source.Id);
            }
        }

        [Fact]
        public void ShouldReverseAConfiguredMemberByMappingScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .AutoReverseConfiguredDataSources()
                    .And
                    .Map(ctx => ctx.Source.Id)
                    .To(pp => pp.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBe(source.Id);
            }
        }

        [Fact]
        public void ShouldReverseAConfiguredMemberByMemberScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(pp => pp.Value)
                    .AndViceVersa();

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBe(source.Id);
            }
        }

        [Fact]
        public void ShouldNotReverseAConfiguredMemberIfGlobalScopeOptedOutAtMappingScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .AutoReverseConfiguredDataSources()
                    .AndWhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .DoNotAutoReverseConfiguredDataSources()
                    .And
                    .Map(p => p.Id, pp => pp.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldNotReverseAConfiguredMemberIfGlobalScopeOptedOutAtMemberScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .AutoReverseConfiguredDataSources()
                    .AndWhenMapping
                    .From<Person>()
                    .To<PublicProperty<Guid>>()
                    .Map(ctx => ctx.Source.Id)
                    .To(pp => pp.Value)
                    .ButNotViceVersa();

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicProperty<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldNotReverseAConfiguredMemberIfMappingScopeOptedOutAtMemberScope()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicField<Guid>>()
                    .AutoReverseConfiguredDataSources()
                    .And
                    .Map(p => p.Id, pf => pf.Value)
                    .ButNotViceVersa();

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicField<Guid>>();

                result.Value.ShouldBe(source.Id);

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldNotReverseAConfiguredConstant()
        {
            using (var mapper = Mapper.CreateNew())
            {
                const string GUID_VALUE = "21EFCF97-C7CF-42C7-B152-1C072E8C3BEA";

                mapper.WhenMapping
                    .AutoReverseConfiguredDataSources()
                    .AndWhenMapping
                    .From<Person>()
                    .To<PublicField<Guid>>()
                    .Map(GUID_VALUE)
                    .To(pf => pf.Value);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicField<Guid>>();

                result.Value.ShouldBe(new Guid(GUID_VALUE));

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBeDefault();
            }
        }

        [Fact]
        public void ShouldNotReverseAConditionalConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .AutoReverseConfiguredDataSources()
                    .AndWhenMapping
                    .From<ProductDto>()
                    .To<PublicProperty<int>>()
                    .If(ctx => ctx.Source.Price > 100)
                    .Map(dto => dto.ProductId, pp => pp.Value);

                var lowPriceSource = new ProductDto { ProductId = "123", Price = 99 };
                var lowPriceResult = mapper.Map(lowPriceSource).ToANew<PublicProperty<int>>();

                lowPriceResult.Value.ShouldBeDefault();

                var highPriceSource = new ProductDto { ProductId = "456", Price = 101 };
                var highPriceResult = mapper.Map(highPriceSource).ToANew<PublicProperty<int>>();

                highPriceResult.Value.ShouldBe(456);

                var lowPriceReverseResult = mapper.Map(lowPriceResult).ToANew<ProductDto>();

                lowPriceReverseResult.ProductId.ShouldBeNull();

                var highPriceReverseResult = mapper.Map(highPriceResult).ToANew<ProductDto>();

                highPriceReverseResult.ProductId.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldAllowReplacementOfImplictlyReversedConfiguredMember()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping.AutoReverseConfiguredDataSources();

                mapper.WhenMapping
                    .From<Person>()
                    .To<PublicTwoFields<Guid, Guid>>()
                    .Map(p => p.Id, ptf => ptf.Value1);

                mapper.WhenMapping
                    .From<PublicTwoFields<Guid, Guid>>()
                    .To<Person>()
                    .Map(ptf => ptf.Value2, p => p.Id);

                var source = new Person { Id = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicTwoFields<Guid, Guid>>();

                result.Value1.ShouldBe(source.Id);
                result.Value2.ShouldBe(source.Id); // ...from reverse of Value2 -> Id

                result.Value2 = Guid.NewGuid();

                var reverseResult = mapper.Map(result).ToANew<Person>();

                reverseResult.Id.ShouldBe(result.Value2);
            }
        }
    }
}