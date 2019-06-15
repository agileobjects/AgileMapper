namespace AgileObjects.AgileMapper.UnitTests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenWorkingWithQueryStrings
    {
        [Fact]
        public void ShouldParseANameValueString()
        {
            var qs = QueryString.Parse("data=value");

            qs.ShouldHaveSingleItem();
            qs.ShouldContainKeyAndValue("data", "value");
        }

        [Fact]
        public void ShouldHandleALeadingQuestionMark()
        {
            var qs = QueryString.Parse("?key=value");

            qs.ShouldHaveSingleItem();
            qs.ShouldContainKeyAndValue("key", "value");
        }

        [Fact]
        public void ShouldExplicitlyConvertToString()
        {
            var qs = QueryString.Parse("key=value");

            var stringQs = (string)qs;

            stringQs.ShouldBe("key=value");
        }

        [Fact]
        public void ShouldEnumerateKeysAndValues()
        {
            var qs = QueryString.Parse("?key1=value1&key2=value2&key3=value3");

            var i = 1;

            foreach (var keyAndValue in qs)
            {
                keyAndValue.Key.ShouldBe("key" + i);
                keyAndValue.Value.ShouldBe("value" + i);

                ++i;
            }
        }

        [Fact]
        public void ShouldActAsAnIDictionary()
        {
            IDictionary<string, string> qs = QueryString.Parse("key1=value1");

            qs.ShouldHaveSingleItem().Key.ShouldBe("key1");

            qs.Add("key2", "value2");

            qs.Keys.Count.ShouldBe(2);
            qs.Keys.First().ShouldBe("key1");
            qs.Keys.Second().ShouldBe("key2");

            qs.Values.Count.ShouldBe(2);
            qs.Values.First().ShouldBe("value1");
            qs.Values.Second().ShouldBe("value2");

            qs.Count.ShouldBe(2);
            qs.First().Key.ShouldBe("key1");
            qs.First().Value.ShouldBe("value1");
            qs.Second().Key.ShouldBe("key2");
            qs.Second().Value.ShouldBe("value2");

            qs.ContainsKey("key1").ShouldBeTrue();
            qs.Remove("key1");
            qs.ContainsKey("key1").ShouldBeFalse();

            qs.ShouldHaveSingleItem().Key.ShouldBe("key2");

            qs.TryGetValue("key2", out var value2).ShouldBeTrue();
            value2.ShouldBe("value2");

            qs["key2"].ShouldBe("value2");
            qs["key2"] = "magic!";
            qs["key2"].ShouldBe("magic!");
        }

        [Fact]
        public void ShouldActAsAnICollection()
        {
            ICollection<KeyValuePair<string, string>> qs = QueryString.Parse("key1=value1");

            qs.IsReadOnly.ShouldBeFalse();
            qs.ShouldHaveSingleItem().Key.ShouldBe("key1");

            qs.Add(new KeyValuePair<string, string>("key2", "value2"));

            qs.Count.ShouldBe(2);
            qs.First().Key.ShouldBe("key1");
            qs.First().Value.ShouldBe("value1");
            qs.Second().Key.ShouldBe("key2");
            qs.Second().Value.ShouldBe("value2");

            var keyValue1 = new KeyValuePair<string, string>("key1", "value1");

            qs.Contains(keyValue1).ShouldBeTrue();
            qs.Remove(keyValue1);
            qs.Contains(keyValue1).ShouldBeFalse();

            qs.ShouldHaveSingleItem().Key.ShouldBe("key2");

            var copyCollection = new KeyValuePair<string, string>[1];

            qs.CopyTo(copyCollection, 0);

            copyCollection[0].Key.ShouldBe("key2");
            copyCollection[0].Value.ShouldBe("value2");

            qs.Clear();
            qs.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldActAsAnIEnumerable()
        {
            IEnumerable qs = QueryString.Parse("key1=value1");

            foreach (KeyValuePair<string, string> keyAndValue in qs)
            {
                keyAndValue.Key.ShouldBe("key1");
                keyAndValue.Value.ShouldBe("value1");
            }
        }
    }
}
