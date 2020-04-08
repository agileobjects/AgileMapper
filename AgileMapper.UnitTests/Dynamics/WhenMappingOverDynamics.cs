namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Common.TestClasses;
    using TestClasses;
    using Xunit;

    public class WhenMappingOverDynamics
    {
        [Fact]
        public void ShouldOverwriteASimpleTypeProperty()
        {
            var source = new { Value = 123 };

            dynamic target = new ExpandoObject();
            target.Value = 456;

            Mapper.Map(source).Over(target);

            Assert.Equal(123, target.Value);
        }

        [Fact]
        public void ShouldOverwriteAnEnumProperty()
        {
            var source = new PublicPropertyStruct<TitleShortlist?>
            {
                Value = TitleShortlist.Mrs
            };

            dynamic target = new ExpandoObject();
            target.Value = Title.Mr;

            Mapper.Map(source).Over(target);

            Assert.Equal(TitleShortlist.Mrs, target.Value);
        }

        [Fact]
        public void ShouldOverwriteFromAStructCollection()
        {
            var source = new[]
            {
                new PublicPropertyStruct<int> { Value = 1 },
                new PublicPropertyStruct<int> { Value = 2 },
                new PublicPropertyStruct<int> { Value = 3 },
            };

            dynamic target = new ExpandoObject();
            target._0_Value = 10;
            target._2_Value = 30;

            Mapper.Map(source).Over(target);

            Assert.Equal(3, ((IDictionary<string, object>)target).Count);
            Assert.Equal(1, target._0_Value);
            Assert.Equal(2, target._1_Value);
            Assert.Equal(3, target._2_Value);
        }

        [Fact]
        public void ShouldHandleAnUnmappableStructCollection()
        {
            var source = new[]
            {
                new PublicPropertyStruct<ProductDto> { Value =  new ProductDto { ProductId = "1" } },
                new PublicPropertyStruct<ProductDto> { Value =  new ProductDto { ProductId = "2" } }
            };

            dynamic target = new ExpandoObject();
            target._0_Value_ProductId = "0";
            target._1_Value_ProductId = "1";

            Mapper.Map(source).Over(target);

            Assert.Equal(2, ((IDictionary<string, object>)target).Count);
            Assert.Equal("0", target._0_Value_ProductId);
            Assert.Equal("1", target._1_Value_ProductId);
        }
    }
}
