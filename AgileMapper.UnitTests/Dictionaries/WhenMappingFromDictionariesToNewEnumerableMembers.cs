namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
#if !NETCOREAPP1_0 && !NET35
    using Microsoft.Extensions.Primitives;
#endif
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingFromDictionariesToNewEnumerableMembers
    {
        [Fact]
        public void ShouldMapToASimpleTypeListFromTypedEntries()
        {
            var source = new Dictionary<string, int>
            {
                ["Value[0]"] = 9,
                ["Value[1]"] = 8,
                ["Value[2]"] = 7
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<List<int>>>();

            result.Value.ShouldBe(9, 8, 7);
        }

        [Fact]
        public void ShouldMapToASimpleTypeCollectionFromConvertibleTypedEntries()
        {
            var now = DateTime.Now;

            var source = new Dictionary<string, DateTime>
            {
                ["Value[0]"] = now,
                ["value[1]"] = now.AddHours(1),
                ["Value[2]"] = now.AddHours(2)
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<string>>>();

            result.Value.ShouldBe(
                now.ToCurrentCultureString(),
                now.AddHours(1).ToCurrentCultureString(),
                now.AddHours(2).ToCurrentCultureString());
        }

        [Fact]
        public void ShouldMapToASimpleTypeCollectionFromAnUntypedEntry()
        {
            var source = new Dictionary<string, object>
            {
                ["Value"] = new[] { '1', '2', '3' }
            };
            var result = Mapper.Map(source).ToANew<PublicField<ICollection<int>>>();

            result.Value.ShouldBe(1, 2, 3);
        }

        [Fact]
        public void ShouldMapToAComplexTypeArrayFromUntypedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0]"] = new Customer { Name = "Mr Pants" },
                ["Value[1]"] = new Person { Name = "Ms Blouse" }
            };
            var result = Mapper.Map(source).ToANew<PublicProperty<Person[]>>();

            result.Value.Length.ShouldBe(2);
            result.Value.First().ShouldBeOfType<Customer>();
            result.Value.First().Name.ShouldBe("Mr Pants");
            result.Value.Second().Name.ShouldBe("Ms Blouse");
        }

        [Fact]
        public void ShouldMapToAComplexTypeCollectionFromTypedDottedEntries()
        {
            var source = new Dictionary<string, string>
            {
                ["Value[0].ProductId"] = "Spade",
                ["Value[0].Price"] = "100.00",
                ["Value[0].HowMega"] = "1.01"
            };
            var result = Mapper.Map(source).ToANew<PublicField<ICollection<MegaProduct>>>();

            result.Value.ShouldHaveSingleItem();
            result.Value.First().ProductId.ShouldBe("Spade");
            result.Value.First().Price.ShouldBe(100.00);
            result.Value.First().HowMega.ShouldBe(1.01);
        }

        [Fact]
        public void ShouldMapToAComplexTypeArrayFromUntypedDottedEntries()
        {
            var source = new Dictionary<string, object>
            {
                ["Value[0].ProductId"] = "Jay",
                ["Value[0].Price"] = "100.00",
                ["Value[0].HowMega"] = "1.01",
                ["Value[1].ProductId"] = "Silent Bob",
                ["Value[1].Price"] = "1000.00",
                ["Value[1].HowMega"] = ".99"
            };
            var result = Mapper.Map(source).ToANew<PublicField<MegaProduct[]>>();

            result.Value.Length.ShouldBe(2);

            result.Value.First().ProductId.ShouldBe("Jay");
            result.Value.First().Price.ShouldBe(100.00);
            result.Value.First().HowMega.ShouldBe(1.01);

            result.Value.Second().ProductId.ShouldBe("Silent Bob");
            result.Value.Second().Price.ShouldBe(1000.00);
            result.Value.Second().HowMega.ShouldBe(0.99);
        }

#if !NETCOREAPP1_0 && !NET35
        // See https://github.com/agileobjects/AgileMapper/issues/50
        // See https://github.com/agileobjects/AgileMapper/issues/51
        [Fact]
        public void ShouldMapStringValuesToAStringArray()
        {
            var source = new Dictionary<string, StringValues>
            {
                ["StringValue"] = new StringValues("123"),
                ["StringArray"] = new StringValues(new[] { "a", "b", "c" }),
                ["IntValue"] = new StringValues("456"),
                ["IntArray"] = new StringValues(new[] { "5", "4", "3" }),
                ["DoubleValue"] = new StringValues("35.4354"),
                ["DoubleArray"] = new StringValues(new[] { "1.23", "2.23", "3.23", "4.23" })
            };

            var result = Mapper.Map(source).ToANew<StringValuesTestDto>();

            result.StringValue.ShouldBe("123");
            result.StringArray.ShouldBe("a", "b", "c");
            result.IntValue.ShouldBe(456);
            result.IntArray.ShouldBe(5, 4, 3);
            result.DoubleValue.ShouldBe(35.4354);
            result.DoubleArray.ShouldBe(1.23, 2.23, 3.23, 4.23);
        }

        #region Helper Class

        public class StringValuesTestDto
        {
            public string StringValue { get; set; }

            public string[] StringArray { get; set; }

            public int IntValue { get; set; }

            public int[] IntArray { get; set; }

            public double DoubleValue { get; set; }

            public double[] DoubleArray { get; set; }
        }

        #endregion
#endif
        [Fact]
        public void ShouldHandleAnUnconvertibleValueForACollection()
        {
            var source = new Dictionary<string, object> { ["Value"] = new Person { Name = "Nope" } };
            var result = Mapper.Map(source).ToANew<PublicProperty<Collection<string>>>();

            result.Value.ShouldBeEmpty();
        }
    }
}