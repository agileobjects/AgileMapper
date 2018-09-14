﻿namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Common;
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

        // See https://github.com/agileobjects/AgileMapper/issues/97
        [Fact]
        public void ShouldDeepCloneAReadOnlyDictionary()
        {
            var source = new PublicReadOnlyProperty<IDictionary<string, string>>(
                new Dictionary<string, string>());

            var cloned = Mapper.DeepClone(source);
        }

        [Fact]
        public void ShouldHandleAnUnconstructableRootTargetType()
        {
            var result = Mapper.Map(new { Test = "Nope" }).ToANew<PublicCtor<int>>();

            result.ShouldBeNull();
        }
    }
}
