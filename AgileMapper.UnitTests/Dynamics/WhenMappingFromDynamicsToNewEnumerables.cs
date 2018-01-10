namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using Api;
    using TestClasses;
    using Xunit;

    public class WhenMappingFromDynamicsToNewEnumerables
    {
        [Fact]
        public void ShouldMapToASimpleTypeArray()
        {
            dynamic source = new ExpandoObject();

            source._0 = 'a';
            source._1 = 'b';
            source._2 = 'c';

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source)).ToANew<string[]>();

            result.ShouldBe("a", "b", "c");
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromComplexTypeEntries()
        {
            dynamic source = new ExpandoObject();

            source._0 = new ProductDto { ProductId = "prod-one" };
            source._1 = new ProductDto { ProductId = "prod-two" };
            source._2 = new ProductDto { ProductId = "prod-three" };

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<Collection<Product>>();

            result.ShouldBe(p => p.ProductId, "prod-one", "prod-two", "prod-three");
        }

        [Fact]
        public void ShouldMapToAComplexTypeListFromFlattenedEntries()
        {
            var guid1 = Guid.NewGuid();
            var guid2 = Guid.NewGuid();
            var guid3 = Guid.NewGuid();

            dynamic source = new ExpandoObject();

            source._0Value1 = guid1;
            source._0Value2 = 123;
            source._1Value1 = guid2;
            source._1Value2 = 456;
            source._2Value1 = guid3;
            source._2Value2 = 789;

            var result = ((ITargetSelector<ExpandoObject>)Mapper.Map(source))
                .ToANew<IList<PublicTwoParamCtor<string, string>>>();

            result.ShouldBe(p => p.Value1, guid1.ToString(), guid2.ToString(), guid3.ToString());
            result.ShouldBe(p => p.Value2, "123", "456", "789");
        }
    }
}
