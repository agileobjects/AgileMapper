﻿namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using Common;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingOnToDictionaries
    {
        [Fact]
        public void ShouldMapASimpleTypeMemberOnToANullUntypedDictionaryEntry()
        {
            var source = new PublicProperty<long> { Value = long.MaxValue };
            var target = new Dictionary<string, object> { ["Value"] = null };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldHaveSingleItem();
            result["Value"].ShouldBe(long.MaxValue);
        }

        [Fact]
        public void ShouldNotOverwriteAnExistingNonDefaultTypedIDictionaryEntry()
        {
            var now = DateTime.Now;
            var source = new PublicField<string> { Value = now.ToCurrentCultureString() };
            IDictionary<string, DateTime> target = new Dictionary<string, DateTime>
            {
                ["Value"] = now.AddHours(1)
            };

            Mapper.Map(source).OnTo(target);

            target.ShouldHaveSingleItem();
            target["Value"].ShouldBe(now.AddHours(1));
        }

        [Fact]
        public void ShouldNotOverwriteAnExistingNonNullUntypedDictionaryEntry()
        {
            var source = new PublicField<short> { Value = short.MinValue };
            var target = new Dictionary<string, object> { ["Value"] = "This is actually a string!" };
            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.ShouldHaveSingleItem();
            result["Value"].ShouldBe("This is actually a string!");
        }

        [Fact]
        public void ShouldMapBetweenDifferentSimpleKeyAndValueTypeDictionaries()
        {
            var source = new Dictionary<string, string>
            {
                ["hfjdk"] = "333", // Parse fails, key -> 0, added
                ["0"] = "000",     // Should be skipped because ^
                ["1"] = "111",     // Should be skipped, target != default
                ["2"] = "222",     // Should be updated, target == default
                ["44"] = "444"     // Should be added
            };
            IDictionary<int, long> target = new Dictionary<int, long>
            {
                [1] = 11,
                [2] = default(long),
                [4] = 44
            };
            Mapper.Map(source).OnTo(target);

            target.Count.ShouldBe(5);
            target[0].ShouldBe(333);
            target[1].ShouldBe(11);
            target[2].ShouldBe(222);
            target[4].ShouldBe(44);
            target[44].ShouldBe(444);
        }

        [Fact]
        public void ShouldMapBetweenDictionaryImplementations()
        {
            var source = new StringKeyedDictionary<Product>
            {
                ["One"] = new Product { ProductId = "One!", Price = 9.99 },
                ["Two"] = new Product { ProductId = "Two!", Price = 10.00 }
            };
            var target = new StringKeyedDictionary<ProductDto>
            {
                ["One"] = new ProductDto { ProductId = "One!" }
            };

            var targetProductDtoOne = target["One"];

            var result = Mapper.Map(source).OnTo(target);

            result.ShouldBeSameAs(target);
            result.Count.ShouldBe(2);

            result.ShouldContainKey("One");
            result["One"].ShouldBeSameAs(targetProductDtoOne);
            result["One"].ProductId.ShouldBe("One!");
            result["One"].Price.ShouldBe(9.99m);

            result.ShouldContainKey("Two");
            result["Two"].ProductId.ShouldBe("Two!");
            result["Two"].Price.ShouldBe(10.00m);
        }
    }
}