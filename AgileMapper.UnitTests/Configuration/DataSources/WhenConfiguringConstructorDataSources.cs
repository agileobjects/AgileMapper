namespace AgileObjects.AgileMapper.UnitTests.Configuration.DataSources
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
    public class WhenConfiguringConstructorDataSources
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantByParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Guid>>()
                    .To<PublicCtor<string>>()
                    .Map("Hello there!")
                    .ToCtor<string>();

                var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicCtor<string>>();

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionByParameterType()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<Guid>>()
                    .To<PublicCtor<string>>()
                    .Map(ctx => ctx.Source.Value.ToString().Substring(0, 10))
                    .ToCtor<string>();

                var source = new PublicProperty<Guid> { Value = Guid.NewGuid() };
                var result = mapper.Map(source).ToANew<PublicCtor<string>>();

                result.Value.ShouldBe(source.Value.ToString().Substring(0, 10));
            }
        }

        [Fact]
        public void ShouldApplyAConfiguredExpressionByParameterName()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<int>>()
                    .To<PublicCtor<long>>()
                    .Map((s, t) => s.Value * 2)
                    .ToCtor("value");

                var source = new PublicProperty<int> { Value = 111 };
                var result = mapper.Map(source).ToANew<PublicCtor<long>>();

                result.Value.ShouldBe(222);
            }
        }

        [Fact]
        public void ShouldApplyMultipleConfiguredSourceValues()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<int>>()
                    .ToANew<CtorTester<int>>()
                    .If(ctx => ctx.Source.Value < 5)
                    .Map(0)
                    .ToCtor<int>()
                    .But
                    .If(ctx => ctx.Source.Value < 10)
                    .Map(5)
                    .ToCtor<int>()
                    .But
                    .If(ctx => ctx.Source.Value < 15)
                    .Map(10)
                    .ToCtor<int>();

                var lessThenFiveSource = new PublicField<int> { Value = 4 };
                var lessthanFiveResult = mapper.Map(lessThenFiveSource).ToANew<CtorTester<int>>();

                lessthanFiveResult.Value.ShouldBe(0);

                var lessThenTenSource = new PublicField<int> { Value = 8 };
                var lessthanTenResult = mapper.Map(lessThenTenSource).ToANew<CtorTester<int>>();

                lessthanTenResult.Value.ShouldBe(5);

                var lessThenFifteenSource = new PublicField<int> { Value = 11 };
                var lessthanFifteenResult = mapper.Map(lessThenFifteenSource).ToANew<CtorTester<int>>();

                lessthanFifteenResult.Value.ShouldBe(10);

                var moreThanFifteenSource = new PublicField<int> { Value = 123 };
                var morethanFifteenResult = mapper.Map(moreThanFifteenSource).ToANew<CtorTester<int>>();

                morethanFifteenResult.Value.ShouldBe(123);
            }
        }

        [Fact]
        public void ShouldApplyMultipleConfiguredComplexTypeSourceValues()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<CtorTester<int>>()
                    .ToANew<CtorTester<int>>()
                    .If(ctx => ctx.Source.Value < 5)
                    .Map(new Address { Line1 = "< 5" })
                    .ToCtor<Address>()
                    .But
                    .If(ctx => ctx.Source.Value < 10)
                    .Map(new Address { Line1 = "< 10" })
                    .ToCtor<Address>()
                    .But
                    .If(ctx => ctx.Source.Value < 15)
                    .Map(new Address { Line1 = "< 15" })
                    .ToCtor<Address>();

                var lessThanFiveSource = new CtorTester<int>(3);
                var lessThanFiveResult = lessThanFiveSource.DeepCloneUsing(mapper);

                lessThanFiveResult.Value.ShouldBe(3);
                lessThanFiveResult.Address.ShouldNotBeNull().Line1.ShouldBe("< 5");

                var lessThanTenSource = new CtorTester<int>(6);
                var lessThanTenResult = lessThanTenSource.DeepCloneUsing(mapper);

                lessThanTenResult.Value.ShouldBe(6);
                lessThanTenResult.Address.ShouldNotBeNull().Line1.ShouldBe("< 10");

                var lessThanFifteenSource = new CtorTester<int>(14);
                var lessThanFifteenResult = lessThanFifteenSource.DeepCloneUsing(mapper);

                lessThanFifteenResult.Value.ShouldBe(14);
                lessThanFifteenResult.Address.ShouldNotBeNull().Line1.ShouldBe("< 15");

                var addressSource = new CtorTester<int>(123, new Address { Line1 = "One!", Line2 = "Two!" });
                var addressResult = addressSource.DeepCloneUsing(mapper);

                addressResult.Value.ShouldBe(123);
                addressResult.Address.ShouldNotBeNull().ShouldNotBeSameAs(addressSource.Address);
                addressResult.Address.Line1.ShouldBe("One!");
                addressResult.Address.Line2.ShouldBe("Two!");

                var noAddressSource = new CtorTester<int>(789);
                var noAddressResult = noAddressSource.DeepCloneUsing(mapper);

                noAddressResult.Value.ShouldBe(789);
                noAddressResult.Address.ShouldBeNull();
            }
        }

        #region Helper Classes

        private class CtorTester<T>
        {
            public CtorTester(T value)
            {
                Value = value;
            }

            public CtorTester(T value, Address address)
            {
                Value = value;
                Address = address;
            }

            public T Value { get; }

            public Address Address { get; }
        }

        #endregion
    }
}