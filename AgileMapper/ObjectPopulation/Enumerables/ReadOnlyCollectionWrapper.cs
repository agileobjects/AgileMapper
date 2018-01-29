namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Extensions.Internal;

    /// <summary>
    /// Wraps a readonly collection to enable efficient creation of a new array. This object
    /// has a readonly implementation of <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the collection.</typeparam>
    public class ReadOnlyCollectionWrapper<T> : IList<T>
    {
        private static readonly ReadOnlyCollection<T> _emptyReadOnlyCollection = new ReadOnlyCollection<T>(Enumerable<T>.EmptyArray);

        private readonly int _numberOfNewItems;
        private T[] _items;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the ReadOnlyCollectionWrapper{T} class.
        /// </summary>
        /// <param name="existingItems">
        /// A read-only IList containing the existing items to retain in the final collection.
        /// </param>
        /// <param name="numberOfNewItems">The number of new items to be added to the existing items.</param>
        public ReadOnlyCollectionWrapper(IList<T> existingItems, int numberOfNewItems)
        {
            _numberOfNewItems = numberOfNewItems;

            var hasExistingItems = existingItems != null;

            if (hasExistingItems)
            {
                _index = existingItems.Count;
            }
            else if (numberOfNewItems == 0)
            {
                _items = Enumerable<T>.EmptyArray;
                return;
            }

            _items = new T[_index + numberOfNewItems];

            if (hasExistingItems)
            {
                _items.CopyFrom(existingItems);
            }
        }

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item.
        /// </summary>
        /// <param name="item">The object to locate in the</param>
        /// <returns>The index of item if found; otherwise, -1.</returns>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public int IndexOf(T item) => Array.IndexOf(_items, item, 0, _index);

        /// <summary>
        /// Inserts an item at the specified index
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void Insert(int index, T item) => ((IList<T>)_items).Insert(index, item);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void RemoveAt(int index) => ((IList<T>)_items).RemoveAt(index);

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int index]
        {
            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            get => _items[index];

            #region ExcludeFromCodeCoverage
#if DEBUG
            [ExcludeFromCodeCoverage]
#endif
            #endregion
            set => _items[index] = value;
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public int Count => _items.Length;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public bool IsReadOnly => true;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add.</param>
        public void Add(T item)
        {
            _items[_index] = item;
            ++_index;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>True if item is found in the collection, otherwise false.</returns>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public bool Contains(T item) => IndexOf(item) != -1;

        /// <summary>
        /// Copies the elements of the System.Collections.Generic.ICollection`1 to an array,
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements copied from the 
        /// collection. The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)_items).CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        /// <returns>
        /// True if the item was successfully removed from the collection, otherwise false. 
        /// This method also returns false if item is not found in the original collection.
        /// </returns>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public bool Remove(T item) => ((ICollection<T>)_items).Remove(item);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            _items = new T[_numberOfNewItems];
            _index = 0;
        }

        #endregion

        #region IEnumerable<T> members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        #region ExcludeFromCodeCoverage
#if DEBUG
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        #endregion

        /// <summary>
        /// Returns an array containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.
        /// </summary>
        /// <returns>An array containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.</returns>
        public T[] ToArray() => _items;

        /// <summary>
        /// Returns a ReadOnlyCollection containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.
        /// </summary>
        /// <returns>
        /// A ReadOnlyCollection containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.
        /// </returns>
        public ReadOnlyCollection<T> ToReadOnlyCollection()
        {
            return (_items != null)
                ? new ReadOnlyCollection<T>(_items)
                : _emptyReadOnlyCollection;
        }
    }
}