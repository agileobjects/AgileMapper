namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;

    internal class EnumerableTypeHelper
    {
        private readonly Type _enumerableType;
        private Type _listType;
        private Type _collectionType;
        private Type _enumerableInterfaceType;

        public EnumerableTypeHelper(Type enumerableType, Type elementType)
        {
            _enumerableType = enumerableType;
            ElementType = elementType;
        }

        public bool IsArray => _enumerableType.IsArray;

        public bool IsList => ListType.IsAssignableFrom(_enumerableType);

        public bool IsCollection => CollectionType.IsAssignableFrom(_enumerableType);

        public bool IsEnumerableInterface => _enumerableType == EnumerableInterfaceType;

        public Type ElementType { get; }

        public Type ListType => GetEnumerableType(ref _listType, typeof(List<>));

        public Type CollectionType => GetEnumerableType(ref _collectionType, typeof(Collection<>));

        private Type EnumerableInterfaceType => GetEnumerableType(ref _enumerableInterfaceType, typeof(IEnumerable<>));

        private Type GetEnumerableType(ref Type typeField, Type openGenericEnumerableType)
            => typeField ?? (typeField = openGenericEnumerableType.MakeGenericType(ElementType));
    }
}