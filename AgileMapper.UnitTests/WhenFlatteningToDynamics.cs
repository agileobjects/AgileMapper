namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using Microsoft.CSharp.RuntimeBinder;
    using TestClasses;
    using Xunit;

    public class WhenFlatteningToDynamics
    {
        [Fact]
        public void ShouldIncludeASimpleTypeMember()
        {
            var source = new PublicProperty<string> { Value = "Flatten THIS" };
            var result = Mapper.Flatten(source).ToDynamic();

            ((object)result).ShouldNotBeNull();
            ((string)result.Value).ShouldBe("Flatten THIS");
        }

        [Fact]
        public void ShouldFlattenASimpleTypeArrayMember()
        {
            var source = new PublicProperty<long[]> { Value = new[] { 1L, 2L, 3L } };
            var result = Mapper.Flatten(source).ToDynamic();

            ((object)result).ShouldNotBeNull();
            ((long)result.Value_0).ShouldBe(1L);
            ((long)result.Value_1).ShouldBe(2L);
            ((long)result.Value_2).ShouldBe(3L);
        }

        [Fact]
        public void ShouldNotIncludeComplexTypeMembers()
        {
            var source = new PublicProperty<PublicField<int>> { Value = new PublicField<int> { Value = 9876 } };
            var result = Mapper.Flatten(source).ToDynamic();

            Should.Throw<RuntimeBinderException>(() => result.Value);
        }

        [Fact]
        public void ShouldFlattenAComplexTypeMember()
        {
            var source = new PublicProperty<PublicField<int>> { Value = new PublicField<int> { Value = 1234 } };
            var result = Mapper.CreateNew().Flatten(source).ToDynamic();

            ((int)result.Value_Value).ShouldBe(1234);
        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            var source = new PublicProperty<PublicField<int>> { Value = null };
            var result = (IDictionary<string, object>)Mapper.Flatten(source).ToDynamic();

            result.ShouldNotContainKey("Value");
            result.ShouldNotContainKey("Value_Value");
        }

        [Fact]
        public void ShouldNotIncludeComplexTypeEnumerableMembers()
        {
            var source = new PublicProperty<IEnumerable<Product>>
            {
                Value = new[]
                {
                    new Product { ProductId = "Summin" }
                }
            };
            var result = Mapper.Flatten(source).ToDynamic();

            Should.Throw<RuntimeBinderException>(() => result.Value);
        }

        [Fact]
        public void ShouldFlattenAComplexTypeEnumerableMember()
        {
            var source = new PublicProperty<IEnumerable<Product>>
            {
                Value = new[]
                {
                    new Product { ProductId = "SumminElse" }
                }
            };
            var result = Mapper.Flatten(source).ToDynamic();

            ((string)result.Value_0_ProductId).ShouldBe("SumminElse");
        }

        [Fact]
        public void ShouldHandleANullComplexTypeEnumerableMemberElement()
        {
            var source = new PublicProperty<IEnumerable<Product>>
            {
                Value = new Product[] { null }
            };
            var result = (IDictionary<string, object>)Mapper.Flatten(source).ToDynamic();

            result.ShouldNotContainKey("Value_0_ProductId");
        }
    }
}
