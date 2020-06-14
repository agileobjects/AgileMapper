namespace AgileObjects.AgileMapper.UnitTests.Extensions.Internal
{
    using System;
    using System.Collections.Generic;
    using AgileMapper.Extensions.Internal;
    using Common;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenFilteringLists
    {
        [Fact]
        public void ShouldFilterAnEmptyArray()
        {
            var source = new int[0];
            var result = source.FilterToArray(i => i > 0);

            result.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldFilterAListWithAllMatchingItems()
        {
            var source = new List<int> { 1, 2, 3 };
            var result = source.FilterToArray(i => i > 0);

            result.ShouldBeSameAs(source);
        }

        [Fact]
        public void ShouldFilterAListWithNoMatchingItems()
        {
            var source = new List<int> { 1, 2, 3 };
            var result = source.FilterToArray(i => i < 0);
            
            result.ShouldBeEmpty();
        }

        [Fact]
        public void ShouldFilterAListWithPartMatchingItems()
        {
            var source = new List<int> { 1, 2, 3 };
            var result = source.FilterToArray(i => i == 2);
            
            result.ShouldHaveSingleItem().ShouldBe(2);
        }

        [Fact]
        public void ShouldReturnReadOnlyResultFromPartMatchFilter()
        {
            var source = new List<int> { 1, 2, 3 };
            var result = source.FilterToArray(i => i > 1);
            
            result.Count.ShouldBe(2);
            result.IsReadOnly.ShouldBeTrue();

            Should.Throw<NotSupportedException>(() => result[0] = 123);
            Should.Throw<NotSupportedException>(() => result.Add(123));
            Should.Throw<NotSupportedException>(() => result.Insert(0, 123));
            Should.Throw<NotSupportedException>(() => result.Remove(2));
            Should.Throw<NotSupportedException>(() => result.RemoveAt(0));
            Should.Throw<NotSupportedException>(() => result.Clear());
        }

        [Fact]
        public void ShouldReturnAListResultFromPartMatchFilter()
        {
            var source = new List<int> { 1, 2, 3 };
            var result = source.FilterToArray(i => i > 1);
            
            result.Count.ShouldBe(2);
            result[0].ShouldBe(2);
            result[1].ShouldBe(3);
            result.Contains(3).ShouldBeTrue();
            result.IndexOf(2).ShouldBe(0);

            var copy = new int[result.Count];
            result.CopyTo(copy, arrayIndex: 0);
            copy.ShouldNotBeSameAs(result);
            copy[0].ShouldBe(2);
            copy[1].ShouldBe(3);

            foreach (var item in result)
            {
                (item > 1).ShouldBeTrue();
            }
        }
    }
}
