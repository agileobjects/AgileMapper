namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewEnumerables
    {
        [Fact]
        public void ShouldMapToASimpleTypeArray()
        {
            dynamic source = new ExpandoObject();

            source._0_ = 'a';
            source._1_ = 'b';
            source._2_ = 'c';

            var result = (string[])Mapper.Map(source).ToANew<string[]>();

            result.ShouldBe("a", "b", "c");
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromComplexTypeEntries()
        {
            dynamic source = new ExpandoObject();

            source._0_ = new ProductDto { ProductId = "prod-one" };
            source._1_ = new ProductDto { ProductId = "prod-two" };
            source._2_ = new ProductDto { ProductId = "prod-three" };

            var result = (Collection<Product>)Mapper.Map(source).ToANew<Collection<Product>>();

            result.ShouldBe(p => p.ProductId, "prod-one", "prod-two", "prod-three");
        }

        [Fact]
        public void ShouldMapToAComplexTypeListFromFlattenedEntries()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            dynamic source = new ExpandoObject();

            source._0_Value1 = guid1;
            source._0_Value2 = 123;
            source._1_Value1 = guid2;
            source._1_Value2 = 456;
            source._2_Value1 = guid3;
            source._2_Value2 = 789;

            var result = (IList<PublicTwoParamCtor<string, string>>)Mapper
                .Map(source)
                .ToANew<IList<PublicTwoParamCtor<string, string>>>();

            result.ShouldBe(p => p.Value1, guid1.ToString(), guid2.ToString(), guid3.ToString());
            result.ShouldBe(p => p.Value2, "123", "456", "789");
        }
    }
}
