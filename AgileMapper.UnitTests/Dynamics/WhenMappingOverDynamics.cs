namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System.Collections.Generic;
    using System.Dynamic;
    using Common;
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

            ((int)target.Value).ShouldBe(123);
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

            ((TitleShortlist)target.Value).ShouldBe(TitleShortlist.Mrs);
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

            ((IDictionary<string, object>)target).Count.ShouldBe(3);

            ((int)target._0_Value).ShouldBe(1);
            ((int)target._1_Value).ShouldBe(2);
            ((int)target._2_Value).ShouldBe(3);
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
            target._1_Value_ProductId = "0";

            Mapper.Map(source).Over(target);

            ((IDictionary<string, object>)target).Count.ShouldBe(2);

            ((string)target._0_Value_ProductId).ShouldBe("0");
            ((string)target._1_Value_ProductId).ShouldBe("0");
        }
    }
}
