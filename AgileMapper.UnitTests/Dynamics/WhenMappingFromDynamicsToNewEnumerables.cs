#if FEATURE_DYNAMIC_ROOT_SOURCE
namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Dynamic;
    using Common.TestClasses;
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

            var result = Mapper.Map(source).ToANew<string[]>();

            Assert.Collection(
               (string[])result,
                str => Assert.Equal("a", str),
                str => Assert.Equal("b", str),
                str => Assert.Equal("c", str));
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromComplexTypeEntries()
        {
            dynamic source = new ExpandoObject();
            source._0 = new ProductDto { ProductId = "prod-one" };
            source._1 = new ProductDto { ProductId = "prod-two" };
            source._2 = new ProductDto { ProductId = "prod-three" };

            var result = Mapper.Map(source).ToANew<Collection<Product>>();

            Assert.Collection(
               (Collection<Product>)result,
                p => Assert.Equal("prod-one", p.ProductId),
                p => Assert.Equal("prod-two", p.ProductId),
                p => Assert.Equal("prod-three", p.ProductId));
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

            var result = Mapper.Map(source).ToANew<IList<PublicTwoParamCtor<string, string>>>();

            Assert.Collection(
               (IList<PublicTwoParamCtor<string, string>>)result,
                ptpc =>
                {
                    Assert.Equal(guid1.ToString(), ptpc.Value1);
                    Assert.Equal("123", ptpc.Value2);
                },
                ptpc =>
                {
                    Assert.Equal(guid2.ToString(), ptpc.Value1);
                    Assert.Equal("456", ptpc.Value2);
                },
                ptpc =>
                {
                    Assert.Equal(guid3.ToString(), ptpc.Value1);
                    Assert.Equal("789", ptpc.Value2);
                });
        }
    }
}
#endif