namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AgileMapper.Extensions.Internal;
    using AgileObjects.AgileMapper.Configuration;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenIgnoringSourceMembersByValueFilterInline
    {
        [Fact]
        public void ShouldIgnoreSourceValuesByMultiClauseTypedValueFiltersOnline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var matchingIntResult = mapper
                    .Map(new PublicField<int> { Value = 123 })
                    .ToANew<PublicProperty<int>>(cfg => cfg
                        .IgnoreSources(c =>
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                matchingIntResult.ShouldNotBeNull();
                matchingIntResult.Value.ShouldBeDefault();

                var matchingStringResult = mapper
                    .Map(new PublicField<string> { Value = "123" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c =>
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                matchingStringResult.ShouldNotBeNull();
                matchingStringResult.Value.ShouldBeNull();

                var nonMatchingIntResult = mapper
                    .Map(new PublicField<int> { Value = 456 })
                    .ToANew<PublicProperty<int>>(cfg => cfg
                        .IgnoreSources(c =>
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                           (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingIntResult.ShouldNotBeNull();
                nonMatchingIntResult.Value.ShouldBe(456);

                var nonMatchingStringResult = mapper
                    .Map(new PublicField<string> { Value = "999" })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c =>
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                            (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingStringResult.ShouldNotBeNull();
                nonMatchingStringResult.Value.ShouldBe("999");

                var nonMatchingTypeResult = mapper
                    .Map(new PublicField<long> { Value = 123L })
                    .ToANew<PublicProperty<string>>(cfg => cfg
                        .IgnoreSources(c =>
                            c.If<string>(str => str == "123") || c.If<int>(i => i == 123) ||
                            (c.If<string>(str => str != "999") && !c.If<DateTime>(dt => dt == DateTime.Today))));

                nonMatchingTypeResult.ShouldNotBeNull();
                nonMatchingTypeResult.Value.ShouldBe("123");
            }
        }

        [Fact]
        public void ShouldHandleNullMemberInANestedSourceValueFilterInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new List<Customer>
                    {
                        new Customer { Name = "Customer 1", Address = new Address { Line1 = "1 Street" } },
                        new MysteryCustomer { Name = "Customer 2"}
                    })
                    .ToANew<IEnumerable<CustomerViewModel>>(cfg => cfg
                        .IgnoreSources(s => s.If<Customer>(c => c.Address.Line1.Length < 2)));

                result.ShouldNotBeNull();
                result.ShouldHaveSingleItem();
                result.First().Name.ShouldBe("Customer 1");
                result.First().AddressLine1.ShouldBe("1 Street");
            }
        }

        [Fact]
        public void ShouldFilterAnEnumerableSourceValueConditionallyInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFields<Product[], Product[]>>()
                    .Over<PublicProperty<List<ProductDto>>>()
                    .Map(ctx => ctx.Source.Value1)
                    .To(t => t.Value)
                    .But
                    .If(ctx => ctx.Source.Value1.None())
                    .Map(ctx => ctx.Source.Value2)
                    .To(t => t.Value);

                var target = new PublicProperty<List<ProductDto>>();

                var bothValuesSource = new PublicTwoFields<Product[], Product[]>
                {
                    Value1 = new[] { new Product { ProductId = "111" } },
                    Value2 = new[] { new Product { ProductId = "222" } }
                };

                mapper.Map(bothValuesSource).Over(target);

                target.Value.ShouldHaveSingleItem().ProductId.ShouldBe("111");
                target.Value.Clear();

                var emptyValue1Source = new PublicTwoFields<Product[], Product[]>
                {
                    Value1 = Enumerable<Product>.EmptyArray,
                    Value2 = new[] { new Product { ProductId = "222" } }
                };

                mapper.Map(emptyValue1Source).Over(target);

                target.Value.ShouldHaveSingleItem().ProductId.ShouldBe("222");
                target.Value.Clear();

                mapper
                    .Map(bothValuesSource)
                    .Over(target, cfg => cfg
                        .IgnoreSources(s => s.If<Product[]>(ps => ps[0].ProductId == "111")));

                target.Value.ShouldBeEmpty();
                target.Value = null;

                mapper
                    .Map(bothValuesSource)
                    .Over(target, cfg => cfg
                        .IgnoreSources(s => s.If<Product[]>(ps => ps[0].ProductId == "111")));

                target.Value.ShouldBeNull();
            }
        }

        [Fact]
        public void ShouldExtendSourceValueFilterConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, long>>()
                    .Over<PublicTwoFields<long, long>>()
                    .IgnoreSources(c => c.If<int>(i => i < 10));

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 4, Value2 = 12L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<long>(l => l > 10L)));

                result1.Value1.ShouldBeDefault(); // int < 10
                result1.Value2.ShouldBeDefault(); // long > 10

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 20, Value2 = 15L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<long>(l => l > 10L)));

                result2.Value1.ShouldBe(20);
                result2.Value2.ShouldBeDefault(); // long > 10

                mapper.InlineContexts().ShouldHaveSingleItem();

                var result3 = mapper
                    .Map(new PublicTwoFieldsStruct<int, long> { Value1 = 20, Value2 = 11L })
                    .Over(new PublicTwoFields<long, long>(), cfg => cfg
                        .IgnoreSources(c => c.If<int>(i => i < 25))
                        .And
                        .IgnoreSources(c => c.If<long>(l => l > 12L)));

                result3.Value1.ShouldBeDefault(); // int < 25
                result3.Value2.ShouldBe(11);

                mapper.InlineContexts().Count.ShouldBe(2);
            }
        }

        [Fact]
        public void ShouldErrorIfDuplicateSourceValueFilterConfiguredInline()
        {
            var configEx = Should.Throw<MappingConfigurationException>(() =>
            {
                using (var mapper = Mapper.CreateNew())
                {
                    mapper.WhenMapping
                        .IgnoreSources(c => c.If<int>(value => value == 555));

                    mapper
                        .Map(new PublicField<int>())
                        .ToANew<PublicProperty<long>>(cfg => cfg
                            .IgnoreSources(c => c.If<int>(value => value == 555)));
                }
            });

            configEx.Message.ShouldContain("Source filter");
            configEx.Message.ShouldContain("If<int>(value => value == 555)");
            configEx.Message.ShouldContain("already been configured");
        }
    }
}
