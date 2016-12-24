namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Extensions;

#if NET_STANDARD
    using System.Reflection;
#endif

    internal class EnumerableTypeHelper
    {
        private bool? _isDictionary;
        private Type _wrapperType;
        private Type _listType;
        private Type _listInterfaceType;
        private Type _collectionType;
        private Type _collectionInterfaceType;
        private Type _enumerableInterfaceType;

        public EnumerableTypeHelper(Type enumerableType, Type elementType)
        {
            EnumerableType = enumerableType;
            ElementType = elementType;
        }

        public bool IsArray => EnumerableType.IsArray;

        public bool IsDictionary
            => _isDictionary ?? (_isDictionary = EnumerableType.IsDictionary()).GetValueOrDefault();

        public bool IsList => ListType.IsAssignableFrom(EnumerableType);

        public bool HasListInterface => ListInterfaceType.IsAssignableFrom(EnumerableType);

        public bool IsCollection => CollectionType.IsAssignableFrom(EnumerableType);

        public bool IsEnumerableInterface => EnumerableType == EnumerableInterfaceType;

        public bool HasCollectionInterface => CollectionInterfaceType.IsAssignableFrom(EnumerableType);

        public bool IsDeclaredReadOnly => IsArray || IsEnumerableInterface;

        public Type EnumerableType { get; }

        public Type ElementType { get; }

        public Type WrapperType => GetEnumerableType(ref _wrapperType, typeof(ReadOnlyCollectionWrapper<>));

        public Type ListType => GetEnumerableType(ref _listType, typeof(List<>));

        public Type ListInterfaceType => GetEnumerableType(ref _listInterfaceType, typeof(IList<>));

        public Type CollectionType => GetEnumerableType(ref _collectionType, typeof(Collection<>));

        public Type CollectionInterfaceType => GetEnumerableType(ref _collectionInterfaceType, typeof(ICollection<>));

        public Type EnumerableInterfaceType => GetEnumerableType(ref _enumerableInterfaceType, typeof(IEnumerable<>));

        private Type GetEnumerableType(ref Type typeField, Type openGenericEnumerableType)
            => typeField ?? (typeField = openGenericEnumerableType.MakeGenericType(ElementType));
    }
}