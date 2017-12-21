namespace AgileObjects.AgileMapper.UnitTests.Dynamics
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
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

            ((Guid?)target.Value.ONEah_ah_ah).ShouldBe(guidOne);
            ((Guid?)target.Value.TWOah_ah_ah).ShouldBe(guidTwo);
            ((string)target.Value.THREEah_ah_ah).ShouldBe("gibblets");
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

            ((string)targetDynamic._0_ProductId).ShouldBe("p-1");
            ((decimal)targetDynamic._0_Price).ShouldBe(10.00m);
            ((string)targetDynamic._0_HowMega).ShouldBe("UBER");

            ((string)targetDynamic._1_ProductId).ShouldBe("p-m1");
            ((decimal)targetDynamic._1_Price).ShouldBe(100.00m);
            ((string)targetDynamic._1_HowMega).ShouldBe("OH SO");

            ((string)targetDynamic._2_ProductId).ShouldBe("p-2");
            ((decimal)targetDynamic._2_Price).ShouldBe(1.99m);
            Should.Throw<RuntimeBinderException>(() => targetDynamic.Value_2_HowMega);
        }
    }
}
