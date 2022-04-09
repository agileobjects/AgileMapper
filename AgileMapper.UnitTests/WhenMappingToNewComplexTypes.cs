namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using AgileMapper.Extensions;
    using Common;
    using Common.TestClasses;
    using MoreTestClasses.Vb;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToNewComplexTypes
    {
        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Value = "Hello there!" };
            var result = Mapper.Map(source).ToANew<PublicProperty<string>>();

            result.Value.ShouldBe(source.Value);
        }

        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var result = Mapper.Map(default(PublicProperty<int>)).ToANew<PublicField<int>>();

            result.ShouldBeNull();
        }

        [Fact]
        public void ShouldMapUsingStaticCloneMethod()
        {
            var source = new Person { Name = "Barney" };
            var result = Mapper.DeepClone(source);

            result.ShouldNotBeSameAs(source);
            result.Name.ShouldBe("Barney");
        }

        [Fact]
        public void ShouldMapUsingInstanceCloneMethod()
        {
            var source = new Person { Name = "Maggie" };
            var result = Mapper.CreateNew().DeepClone(source);

            result.ShouldNotBeSameAs(source);
            result.Name.ShouldBe("Maggie");
        }

        [Fact]
        public void ShouldCopyAnIntValue()
        {
            var source = new PublicField<int> { Value = 123 };
            var result = Mapper.Map(source).ToANew<PublicProperty<int>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldCopyAStringValue()
        {
            var source = new PublicProperty<string> { Value = "Oi 'Arry!" };
            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Oi 'Arry!");
        }

        [Fact]
        public void ShouldMapFromASimpleTypeToObject()
        {
            var source = new PublicProperty<string> { Value = "Oi 'Arold!" };
            var result = Mapper.Map(source).ToANew<PublicField<object>>();

            result.Value.ShouldBe("Oi 'Arold!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/11
        [Fact]
        public void ShouldMapFromAnInterface()
        {
            IPublicInterface<string> source = new PublicImplementation<string>
            {
                Value = "Interfaces!"
            };

            var result = Mapper.Map(source).ToANew<PublicField<string>>();

            result.Value.ShouldBe("Interfaces!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/66
        [Fact]
        public void ShouldMapToAGivenTypeObject()
        {
            var source = new PublicProperty<string>
            {
                Value = "kjubfelkjnds;lkmm"
            };
            var result = Mapper.Map(source).ToANew(typeof(PublicField<string>));

            result.ShouldBeOfType<PublicField<string>>();
            ((PublicField<string>)result).Value.ShouldBe("kjubfelkjnds;lkmm");
        }

        [Fact]
        public void ShouldHandleAnUnconstructableRootTargetType()
        {
            var result = Mapper.Map(new { Test = "Nope" }).ToANew<PublicCtor<int>>();

            result.ShouldBeNull();
        }

        [Fact]
        public void ShouldConditionallyUseConstructorsWhereArgumentsAreNull()
        {
            var noAddressSource = new CtorTester("Test 1");
            var noAddressResult = noAddressSource.DeepClone();

            noAddressResult.Value.ShouldBe("Test 1");
            noAddressResult.Address.ShouldBeNull();

            var addressSource = new CtorTester("Test 2", new Address { Line1 = "Line 1!" });
            var addressResult = addressSource.DeepClone();

            addressResult.Value.ShouldBe("Test 2");
            addressResult.Address.ShouldNotBeNull();
            addressResult.Address.ShouldNotBeSameAs(addressSource.Address);
            addressResult.Address.Line1.ShouldBe("Line 1!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/221
        [Fact]
        public void ShouldIgnoreUnmappableSourceIndexedProperties()
        {
            var source = new PublicNamedIndex<PublicField<string>, PublicField<int>>
            {
                Value1ToReturn = new PublicField<string> { Value = "Test" }
            };

            var result = Mapper
                .Map(source)
                .ToANew<PublicTwoFields<PublicField<string>, PublicField<int>>>();

            result.ShouldNotBeNull().Value1.ShouldNotBeNull().Value.ShouldBe("Test");
            result.ShouldNotBeNull().Value2.ShouldBeNull();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/221
        [Fact]
        public void ShouldIgnoreUnmappableTargetIndexedProperties()
        {
            var source = new PublicTwoFields<PublicField<int>, PublicField<string>>
            {
                Value1 = new PublicField<int> { Value = 999 },
                Value2 = new PublicField<string> { Value = "C ya!" }
            };

            var target = new PublicNamedIndex<PublicField<int>, PublicField<string>>
            {
                Value1ToReturn = new PublicField<int>()
            };

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldNotBeNull().get_Value1().ShouldNotBeNull().Value.ShouldBe(999);
            result.ShouldNotBeNull().Value2SetValue.ShouldBeNull();
        }

        #region Helper Classes

        private class CtorTester
        {
            public CtorTester(string value)
            {
                Value = value;
            }

            public CtorTester(string value, Address address)
            {
                Value = value;
                Address = address ?? throw new ArgumentNullException(nameof(address));
            }

            public string Value { get; }

            public Address Address { get; }
        }

        #endregion
    }
}
