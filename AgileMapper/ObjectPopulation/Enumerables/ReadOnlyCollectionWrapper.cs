namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Extensions;

    /// <summary>
    /// Wraps a readonly collection to enable efficient creation of a new array. This object
    /// has a readonly implementation of <see cref="IList{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the collection.</typeparam>
    public class ReadOnlyCollectionWrapper<T> : IList<T>
    {
        private readonly IEnumerable<T> _existingItems;
        private readonly T[] _newItems;
        private int _index;

        /// <summary>
        /// Initializes a new instance of the ReadOnlyCollectionWrapper{T} class.
        /// </summary>
        /// <param name="existingItems">The existing items to retain in the final collection.</param>
        /// <param name="numberOfNewItems">The number of new items to be added to the existing items.</param>
        public ReadOnlyCollectionWrapper(IEnumerable<T> existingItems, int numberOfNewItems)
            : this(numberOfNewItems)
        {
            _existingItems = existingItems;
        }

        /// <summary>
        /// Initializes a new instance of the ReadOnlyCollectionWrapper{T} class.
        /// </summary>
        /// <param name="numberOfNewItems">The number of new items to be added to the collection.</param>
        public ReadOnlyCollectionWrapper(int numberOfNewItems)
        {
            _newItems = new T[numberOfNewItems];
            _index = 0;
        }

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item.
        /// </summary>
        /// <param name="item">The object to locate in the</param>
        /// <returns>The index of item if found; otherwise, -1.</returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public int IndexOf(T item) => Array.IndexOf(_newItems, item, 0, _newItems.Length);

        /// <summary>
        /// Inserts an item at the specified index
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert.</param>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void Insert(int index, T item) => ((IList<T>)_newItems).Insert(index, item);

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void RemoveAt(int index) => ((IList<T>)_newItems).RemoveAt(index);

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public T this[int index]
        {
            get { return _newItems[index]; }
            set { _newItems[index] = value; }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public int Count => _newItems.Length + (_existingItems?.Count() ?? 0);

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
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
            _newItems[_index] = item;
            ++_index;
        }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate.</param>
        /// <returns>True if item is found in the collection, otherwise false.</returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
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
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)_newItems).CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove.</param>
        /// <returns>
        /// True if the item was successfully removed from the collection, otherwise false. 
        /// This method also returns false if item is not found in the original collection.
        /// </returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public bool Remove(T item) => ((ICollection<T>)_newItems).Remove(item);

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public void Clear() => ((ICollection<T>)_newItems).Clear();

        #endregion

        #region IEnumerable<T> members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_newItems).GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
        #region ExcludeFromCodeCoverage
#if !NET_STANDARD
        [ExcludeFromCodeCoverage]
#endif
        #endregion
        IEnumerator IEnumerable.GetEnumerator() => _newItems.GetEnumerator();

        #endregion

        /// <summary>
        /// Returns an array containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.
        /// </summary>
        /// <returns>An array containing the contents of the <see cref="ReadOnlyCollectionWrapper{T}"/>.</returns>
        public T[] ToArray()
        {
            if (_existingItems == null)
            {
                return _newItems;
            }

            return _existingItems.Concat(_newItems).ToArray();
        }
    }
}