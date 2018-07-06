namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class EnumerableTypeHelper
    {
        private bool? _isDictionary;
        private Type _listType;
        private Type _listInterfaceType;
        private Type _collectionType;
        private Type _readOnlyCollectionType;
        private Type _collectionInterfaceType;
        private Type _enumerableInterfaceType;

        public EnumerableTypeHelper(IQualifiedMember member)
            : this(member.Type, member.ElementType)
        {
        }

        public EnumerableTypeHelper(Type enumerableType, Type elementType = null)
        {
            EnumerableType = enumerableType;
            ElementType = elementType ?? enumerableType.GetEnumerableElementType();
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

        public bool IsDeclaredReadOnly
            => IsReadOnly || IsEnumerableInterface || IsReadOnlyCollectionInterface();

        private bool IsReadOnlyCollectionInterface()
        {
#if NET_STANDARD
            return EnumerableType.IsClosedTypeOf(typeof(IReadOnlyCollection<>));
#else
            return EnumerableType.IsInterface() &&
                  (EnumerableType.Name == "IReadOnlyCollection`1") &&
                   EnumerableType.IsFromBcl();
#endif
        }

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

        public Expression GetNewInstanceCreation()
        {
            return IsReadOnly || EnumerableType.IsInterface()
                ? Expression.New(ListType)
                : GetEmptyInstanceCreation();
        }

        public Expression GetEmptyInstanceCreation(Type enumerableType = null)
        {
            if ((enumerableType == EnumerableType) || (enumerableType == null))
            {
                return EnumerableType.GetEmptyInstanceCreation(ElementType, this);
            }

            return enumerableType.GetEmptyInstanceCreation(ElementType);
        }

        public Expression GetWrapperConstruction(Expression existingItems, Expression newItemsCount)
        {
            return Expression.New(
                WrapperType.GetPublicInstanceConstructor(ListInterfaceType, typeof(int)),
                existingItems,
                newItemsCount);
        }

        public Expression GetEnumerableConversion(Expression instance, bool allowEnumerableAssignment)
        {
            if (instance.Type.IsAssignableTo(EnumerableType) &&
               (allowEnumerableAssignment || ValueIsNotEnumerableInterface(instance)))
            {
                return instance;
            }

            if (IsArray)
            {
                return instance.WithToArrayCall(ElementType);
            }

            if (IsReadOnlyCollection)
            {
                return instance.WithToReadOnlyCollectionCall(ElementType);
            }

            if (IsCollection)
            {
                return instance.WithToCollectionCall(ElementType);
            }

            return instance.WithToListLinqCall(ElementType);
        }

        private static bool ValueIsNotEnumerableInterface(Expression instance)
            => instance.Type != typeof(IEnumerable<>).MakeGenericType(instance.Type.GetEnumerableElementType());

        public Expression GetCountFor(Expression instance, Type countType = null)
            => instance.GetCount(countType, exp => CollectionInterfaceType);
    }
}