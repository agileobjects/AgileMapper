namespace AgileObjects.AgileMapper.UnitTests.Dictionaries
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
#if !NET_STANDARD
    using Microsoft.Extensions.Primitives;
#endif
    using TestClasses;
    using Xunit;

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

#if !NET_STANDARD

        // See https://github.com/agileobjects/AgileMapper/issues/50
        [Fact]
        public void ShouldMapStringValuesToAStringArray()
        {
            var source = new Dictionary<string, StringValues>
            {
                ["WidgetId"] = new StringValues("123"),
                ["ClientId"] = new StringValues("456"),
                ["TestPayload"] = new StringValues(new[] { "a", "b", "c" })
            };

            var result = Mapper.Map(source).ToANew<StringValuesTestDto>();

            result.WidgetId.ShouldBe("123");
            result.ClientId.ShouldBe("456");
            result.TestPayload.ShouldBe("a", "b", "c");
        }

        #region Helper Class

        public class StringValuesTestDto
        {
            public string WidgetId { get; set; }

            public string ClientId { get; set; }

            public string[] TestPayload { get; set; }
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