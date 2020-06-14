namespace AgileObjects.AgileMapper.Extensions.Internal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class FilteredArray<T> : IList<T>
    {
        private readonly IList<T> _items;

        public FilteredArray(IList<T> items, int itemCount)
        {
            _items = items;
            Count = itemCount;
        }

        public int Count { get; }

        public bool IsReadOnly => true;

        public T this[int index]
        {
            get => _items[index];
            set => throw new NotSupportedException();
        }

        public void Add(T item) => throw new NotSupportedException();

        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = 0; i < Count; ++i)
            {
                array[arrayIndex + i] = _items[i];
            }
        }

        public int IndexOf(T item) => _items.IndexOf(item);

        public void Insert(int index, T item) => throw new NotSupportedException();

        public bool Remove(T item) => throw new NotSupportedException();

        public void RemoveAt(int index) => throw new NotSupportedException();

        public void Clear() => throw new NotSupportedException();

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; ++i)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
