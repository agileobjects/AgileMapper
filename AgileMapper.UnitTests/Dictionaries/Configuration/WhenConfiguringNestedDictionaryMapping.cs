namespace AgileObjects.AgileMapper.UnitTests.Dictionaries.Configuration
{
    using System.Collections.Generic;
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringNestedDictionaryMapping
    {
        [Fact]
        public void ShouldPopulateANestedStringFromAConfiguredNestedObjectEntry()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicField<Dictionary<string, object>>>()
                    .ToANew<Person>()
                    .Map(ctx => ctx.Source.Value)
                    .To(p => p.Address);

                var source = new PublicField<Dictionary<string, object>>
                {
                    Value = new Dictionary<string, object>
                    {
                        ["Line1"] = "6478 Nested Drive"
                    }
                };
                var result = mapper.Map(source).ToANew<Person>();

                result.Address.ShouldNotBeNull();
                result.Address.Line1.ShouldBe("6478 Nested Drive");
            }
        }

        // See https://github.com/agileobjects/AgileMapper/issues/133
        [Fact]
        public void ShouldApplyANestedDictionaryToARootTarget()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<Issue133.Source.Wrapper>()
                    .ToDictionaries
                    .Map(ctx => ctx.Source.Map)
                    .ToTarget();

                var source = new Issue133.Source.Wrapper
                {
                    Map =
                    {
                        { "first", new Issue133.Source.Data { Name = "First" } },
                        { "second", new Issue133.Source.Data { Name = "Second" } }
                    }
                };

                var result = mapper.Map(source).ToANew<Dictionary<string, Issue133.Target.Data>>();

                result.ShouldNotBeNull();
                result.Count.ShouldBe(3);

                result.ShouldContainKey("Map");
                result.ShouldContainKey("first");
                result.ShouldContainKey("second");

                result["Map"].Name.ShouldBeNull();
                result["first"].Name.ShouldBe("First");
                result["second"].Name.ShouldBe("Second");
            }
        }

        private static class Issue133
        {
            public static class Source
            {
                public class Wrapper
                {
                    public IDictionary<string, Data> Map { get; } = new Dictionary<string, Data>();
                }

                public class Data
                {
                    public string Name { get; set; }
                }
            }

            public static class Target
            {
                public class Data
                {
                    public string Name { get; set; }
                }
            }
        }
    }
}
