﻿namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOverComplexTypes
    {
        [Fact]
        public void ShouldReuseAnExistingTargetObject()
        {
            var source = new PublicField<string>();
            var target = new PublicProperty<string>();

            var result = Mapper.Map(source).Over(target);

            result.ShouldBe(target);
        }

        [Fact]
        public void ShouldMapFromAnAnonymousType()
        {
            var source = new { Id = Guid.NewGuid(), Name = "Mr Pants" };
            var target = new Person { Id = Guid.NewGuid(), Name = "Mrs Trousers" };
            var result = Mapper.Map(source).Over(target);

            result.Id.ShouldBe(source.Id);
            result.Name.ShouldBe(source.Name);
        }

        [Fact]
        public void ShouldOverwriteAnExistingSimpleTypePropertyValue()
        {
            var source = new PublicField<int> { Value = 123 };
            var target = new PublicProperty<int> { Value = 789 };

            Mapper.Map(source).Over(target);

            target.Value.ShouldBe(123);
        }

        [Fact]
        public void ShouldOverwriteExistingSimpleTypePropertyValues()
        {
            var source = new Address { Line1 = "Source 1", Line2 = "Source 2" };
            var target = new Address { Line1 = "Target 1", Line2 = "Target 2" };

            Mapper.Map(source).Over(target);

            target.Line1.ShouldBe("Source 1");
            target.Line2.ShouldBe("Source 2");
        }

        [Fact]
        public void ShouldNullAnExistingSimpleTypePropertyValue()
        {
            var source = new PublicProperty<double?> { Value = null };
            var target = new PublicField<double?> { Value = 537.0 };

            Mapper.Map(source).Over(target);

            target.Value.ShouldBeNull();
        }

        [Fact]
        public void ShouldHandleANullSourceObject()
        {
            var target = new PublicProperty<int>();
            var result = Mapper.Map(default(PublicField<int>)).Over(target);

            result.ShouldBe(target);
        }

        [Fact]
        public void ShouldOverwriteAMemberWithAMatchingCtorParameter()
        {
            var source = new PublicTwoFields<int, int> { Value1 = 123, Value2 = 456 };
            var target = new PublicTwoParamCtor<int, int>(111, 222);

            Mapper.Map(source).Over(target);

            target.Value1.ShouldBe(111);
            target.Value2.ShouldBe(456);
        }
    }
}
