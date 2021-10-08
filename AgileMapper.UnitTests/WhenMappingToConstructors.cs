namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingToConstructors
    {
        [Fact]
        public void ShouldUseADefaultConstructor()
        {
            var source = new PublicField<string>();
            var result = Mapper.Map(source).ToANew<PublicProperty<string>>();

            result.ShouldNotBeNull();
        }

        [Fact]
        public void ShouldUseAParameterisedConstructor()
        {
            var source = new PublicGetMethod<string>("Barney");
            var result = Mapper.Map(source).ToANew<PublicCtor<string>>();

            result.ShouldNotBeNull();
            result.Value.ShouldBe("Barney");
        }

        [Fact]
        public void ShouldConvertASimpleTypeConstructorArgument()
        {
            var source = new PublicGetMethod<string>("80.6537");
            var result = Mapper.Map(source).ToANew<PublicCtor<decimal>>();

            result.Value.ShouldBe(80.6537);
        }

        [Fact]
        public void ShouldUseAMultipleParameterConstructor()
        {
            var source = new { Value1 = 123, Value2 = 456 };
            var result = Mapper.Map(source).ToANew<PublicTwoParamCtor<byte?, string>>();

            result.Value1.ShouldBe(123);
            result.Value2.ShouldBe("456");
        }

        [Fact]
        public void ShouldUseTheGreediestConstructor()
        {
            var value1OnlySource = new { Value1 = "Good bye" };

            var value1OnlyResult = Mapper.Map(value1OnlySource).ToANew<MultipleConstructors<string, string>>();
            value1OnlyResult.ShouldNotBeNull();
            value1OnlyResult.Value1.ShouldBe("Good bye");

            var value1And2Source = new { Value1 = "Hello", Value2 = "Again!" };
            var value1And2Result = Mapper.Map(value1And2Source).ToANew<MultipleConstructors<string, string>>();
            value1And2Result.ShouldNotBeNull();
            value1And2Result.Value1.ShouldBe("Hello");
            value1And2Result.Value2.ShouldBe("Again!");
        }

        [Fact]
        public void ShouldUseAComplexTypeConstructorParameter()
        {
            var source = new PublicField<Address> { Value = new Address { Line1 = "Over there" } };
            var result = Mapper.Map(source).ToANew<PublicCtor<Address>>();

            result.ShouldNotBeNull();
            result.Value.ShouldNotBeNull();
            result.Value.Line1.ShouldBe("Over there");
        }

        [Fact]
        public void ShouldIgnoreConstructorsWithNoUseableDataSource()
        {
            var source = new { Value1 = "Yo Bo", Value2 = DateTime.Today };
            var result = Mapper.Map(source).ToANew<MultipleConstructors<string, byte>>();

            result.ShouldNotBeNull();
            result.Value1.ShouldBe("Yo Bo");
            result.Value2.ShouldBeDefault();
        }

        // See https://github.com/agileobjects/AgileMapper/issues/9
        [Fact]
        public void ShouldIgnoreACopyConstructor()
        {
            var source = new CopyConstructor { StringValue = "Copy!" };
            var result = Mapper.Map(source).ToANew<CopyConstructor>();

            result.StringValue.ShouldBe("Copy!");
        }

        // See https://github.com/agileobjects/AgileMapper/issues/139
        [Fact]
        public void ShouldPopulateMembersMatchingUnusedConstructorParameters()
        {
            var source = new { Value1 = 123 };
            var result = Mapper.Map(source).ToANew<MultipleUnusedConstructors<int, int>>();

            result.Value1.ShouldBe(123);
        }

        [Fact]
        public void ShouldDefaultCollectionParametersToEmpty()
        {
            var source = new PublicField<IList<Address>>();
            var result = Mapper.Map(source).ToANew<PublicCtor<ICollection<Address>>>();

            result.Value.ShouldBeEmpty();
        }

        #region Helper Classes

        // ReSharper disable ClassNeverInstantiated.Local
        // ReSharper disable UnusedMember.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable MemberCanBePrivate.Local
        private class MultipleConstructors<T1, T2>
        {
            public MultipleConstructors()
            {
            }

            public MultipleConstructors(T1 value1)
            {
                Value1 = value1;
            }

            public MultipleConstructors(T1 value1, T2 value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public T1 Value1 { get; }

            public T2 Value2 { get; }
        }

        private class MultipleUnusedConstructors<T1, T2>
        {
            public MultipleUnusedConstructors()
            {
            }

            public MultipleUnusedConstructors(T1 value1, T2 value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            public T1 Value1 { get; set; }

            public T2 Value2 { get; set; }
        }

        private class CopyConstructor
        {
            public CopyConstructor()
            {
            }

            public CopyConstructor(CopyConstructor otherInstance)
            {
                StringValue = otherInstance.StringValue;
            }

            // ReSharper disable once MemberCanBePrivate.Local
            public string StringValue { get; set; }
        }
        // ReSharper restore MemberCanBePrivate.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedMember.Local
        // ReSharper restore ClassNeverInstantiated.Local

        #endregion
    }
}
