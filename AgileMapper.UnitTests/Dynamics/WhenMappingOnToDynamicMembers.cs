namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using Common;
    using Common.TestClasses;
    using Microsoft.CSharp.RuntimeBinder;
    using TestClasses;
    using Xunit;

    public class WhenMappingOnToDynamicMembers
    {
        [Fact]
        public void ShouldMergeFromANestedSimpleTypedDictionary()
        {
            var guidOne = Guid.NewGuid();
            var guidTwo = Guid.NewGuid();

            var source = new PublicProperty<Dictionary<string, Guid?>>
            {
                Value = new Dictionary<string, Guid?> { ["ONEah-ah-ah"] = guidOne, ["TWOah-ah-ah"] = guidTwo }
            };

            dynamic targetDynamic = new ExpandoObject();
            targetDynamic.ONEah_ah_ah = guidOne;
            targetDynamic.TWOah_ah_ah = guidTwo;
            targetDynamic.THREEah_ah_ah = "gibblets";

            var target = new PublicField<dynamic> { Value = targetDynamic };

            Mapper.Map(source).Over(target);
            
            Assert.Equal(guidOne, target.Value.ONEah_ah_ah);
            Assert.Equal(guidTwo, target.Value.TWOah_ah_ah);
            Assert.Equal("gibblets", target.Value.THREEah_ah_ah);
        }

        [Fact]
        public void ShouldMapFromANestedComplexTypeEnumerableOnToNestedMembers()
        {
            var source = new PublicField<ProductDto[]>
            {
                Value = new[]
                {
                    new ProductDto { ProductId = "p-1", Price = 10.00m },
                    new ProductDtoMega { ProductId = "p-m", Price = 100.00m, HowMega = "OH SO" },
                    new ProductDto { ProductId = "p-2", Price = 1.99m }
                }
            };

            dynamic targetDynamic = new ExpandoObject();
            targetDynamic._0_ProductId = default(string);
            targetDynamic._0_Price = default(double?);
            targetDynamic._0_HowMega = "UBER";

            targetDynamic._1_ProductId = "p-m1";
            targetDynamic._1_Price = default(int?);

            var target = new PublicField<dynamic> { Value = targetDynamic };

            Mapper.Map(source).OnTo(target);

            Assert.Equal("p-1", targetDynamic._0_ProductId);
            Assert.Equal(10.00m, targetDynamic._0_Price);
            Assert.Equal("UBER", targetDynamic._0_HowMega);

            Assert.Equal("p-m1", targetDynamic._1_ProductId);
            Assert.Equal(100.00m, targetDynamic._1_Price);
            Assert.Equal("OH SO", targetDynamic._1_HowMega);

            Assert.Equal("p-2", targetDynamic._2_ProductId);
            Assert.Equal(1.99m, targetDynamic._2_Price);
            Should.Throw<RuntimeBinderException>(() => targetDynamic.Value_2_HowMega);
        }
    }
}
