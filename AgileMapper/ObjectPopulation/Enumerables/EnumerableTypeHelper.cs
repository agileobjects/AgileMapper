namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Extensions;

    internal class EnumerableTypeHelper
    {
        private readonly Type _enumerableType;
        private Type _wrapperType;
        private Type _listType;
        private Type _listInterfaceType;
        private Type _collectionType;
        private Type _collectionInterfaceType;
        private Type _enumerableInterfaceType;

        public EnumerableTypeHelper(Type enumerableType, Type elementType)
        {
            _enumerableType = enumerableType;
            ElementType = elementType;
        }

        public bool IsDictionary => _enumerableType.IsDictionary();

        public bool IsArray => _enumerableType.IsArray;

        public bool IsList => ListType.IsAssignableFrom(_enumerableType);

        public bool HasListInterface => ListInterfaceType.IsAssignableFrom(_enumerableType);

        public bool IsCollection => CollectionType.IsAssignableFrom(_enumerableType);

        public bool IsEnumerableInterface => _enumerableType == EnumerableInterfaceType;

        public bool HasCollectionInterface => CollectionInterfaceType.IsAssignableFrom(_enumerableType);

        public bool IsDeclaredReadOnly => IsArray || IsEnumerableInterface;

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