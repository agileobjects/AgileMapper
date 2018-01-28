namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections.Generic;
    using TestClasses;
    using Xunit;

    public class WhenFlatteningToDictionaries
    {
        [Fact]
        public void ShouldIncludeASimpleTypeMember()
        {
            var source = new PublicProperty<string> { Value = "Flatten THIS" };
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotBeNull();
            result["Value"].ShouldBe("Flatten THIS");
        }

        [Fact]
        public void ShouldFlattenASimpleTypeArrayMember()
        {
            var source = new PublicProperty<long[]> { Value = new[] { 1L, 2L, 3L } };
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotBeNull();
            ((long)result["Value[0]"]).ShouldBe(1L);
            ((long)result["Value[1]"]).ShouldBe(2L);
            ((long)result["Value[2]"]).ShouldBe(3L);
        }

        [Fact]
        public void ShouldNotIncludeComplexTypeMembers()
        {
            var source = new PublicProperty<PublicField<int>> { Value = new PublicField<int> { Value = 9876 } };
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotContainKey("Value");
        }

        [Fact]
        public void ShouldFlattenAComplexTypeMember()
        {
            var source = new PublicProperty<PublicField<int>> { Value = new PublicField<int> { Value = 1234 } };
            var result = Mapper.CreateNew().Flatten(source).ToDictionary();

            ((int)result["Value.Value"]).ShouldBe(1234);
        }

        [Fact]
        public void ShouldHandleANullComplexTypeMember()
        {
            var source = new PublicProperty<PublicField<int>> { Value = null };
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotContainKey("Value");
            result.ShouldNotContainKey("Value.Value");
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
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotContainKey("Value");
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

            var result = Mapper.Flatten(source).ToDictionary(cfg => cfg
                .ForDictionaries.UseFlattenedMemberNames());

            ((string)result["Value[0]ProductId"]).ShouldBe("SumminElse");
        }

        [Fact]
        public void ShouldHandleANullComplexTypeEnumerableMemberElement()
        {
            var source = new PublicProperty<IEnumerable<Product>>
            {
                Value = new Product[] { null }
            };
            var result = Mapper.Flatten(source).ToDictionary();

            result.ShouldNotContainKey("Value[0].ProductId");
        }
    }
}
