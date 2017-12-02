namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq.Expressions;
    using Extensions;
    using NetStandardPolyfills;

    internal class EnumerableTypeHelper
    {
        private bool? _isDictionary;
        private Type _listType;
        private Type _listInterfaceType;
        private Type _collectionType;
        private Type _readOnlyCollectionType;
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

        public bool IsList => EnumerableType.IsAssignableTo(ListType);

        public bool HasListInterface => EnumerableType.IsAssignableTo(ListInterfaceType);

        public bool IsCollection => EnumerableType.IsAssignableTo(CollectionType);

        public bool IsReadOnlyCollection => EnumerableType == ReadOnlyCollectionType;

        public bool IsEnumerableInterface => EnumerableType == EnumerableInterfaceType;

        public bool HasCollectionInterface => EnumerableType.IsAssignableTo(CollectionInterfaceType);

        public bool IsReadOnly => IsArray || IsReadOnlyCollection;

        public bool IsDeclaredReadOnly => IsReadOnly || IsEnumerableInterface;

        public Type EnumerableType { get; }

        public Type ElementType { get; }

        public Type ListType => GetEnumerableType(ref _listType, typeof(List<>));

        public Type ListInterfaceType => GetEnumerableType(ref _listInterfaceType, typeof(IList<>));

        public Type CollectionType => GetEnumerableType(ref _collectionType, typeof(Collection<>));

        public Type ReadOnlyCollectionType => GetEnumerableType(ref _readOnlyCollectionType, typeof(ReadOnlyCollection<>));

        public Type CollectionInterfaceType => GetEnumerableType(ref _collectionInterfaceType, typeof(ICollection<>));

        public Type EnumerableInterfaceType => GetEnumerableType(ref _enumerableInterfaceType, typeof(IEnumerable<>));

        private Type GetEnumerableType(ref Type typeField, Type openGenericEnumerableType)
            => typeField ?? (typeField = openGenericEnumerableType.MakeGenericType(ElementType));

        public Type WrapperType => typeof(ReadOnlyCollectionWrapper<>).MakeGenericType(ElementType);

        public Expression GetEmptyInstanceCreation()
        {
            if (IsReadOnly || EnumerableType.IsInterface())
            {
                return Expression.New(ListType);
            }

            return EnumerableType.GetEmptyInstanceCreation(ElementType);
        }

        public Expression GetWrapperConstruction(Expression existingItems, Expression newItemsCount)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(
                WrapperType.GetPublicInstanceConstructor(ListInterfaceType, typeof(int)),
                existingItems,
                newItemsCount);
        }

        public Expression GetEnumerableConversion(Expression instance)
        {
            if (IsArray)
            {
                return instance.WithToArrayCall(ElementType);
            }

            if (IsReadOnlyCollection)
            {
                return instance.WithToReadOnlyCollectionCall(ElementType);
            }

            return instance.WithToListCall(ElementType);
        }
    }
}