namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class PublicEnumerable<T> : IEnumerable<T>
    {
        private readonly List<T> _items;

        public PublicEnumerable()
            : this(new List<T>())
        {
        }

        private PublicEnumerable(List<T> items)
        {
            _items = items;
        }

        public static PublicEnumerable<T> Parse(string values)
        {
            return new PublicEnumerable<T>(values
                .Split(',')
                .Select(v => (T)Convert.ChangeType(v, typeof(T)))
                .ToList());
        }

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}