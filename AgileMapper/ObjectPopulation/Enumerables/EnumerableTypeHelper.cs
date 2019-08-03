namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using TypeConversion;

    internal class EnumerableTypeHelper
    {
        private bool? _isDictionary;
        private bool? _couldBeReadOnly;
        private Type _listType;
        private Type _listInterfaceType;
        private Type _collectionType;
        private Type _hashSetType;
        private Type _readOnlyCollectionType;
        private Type _collectionInterfaceType;
        private Type _enumerableInterfaceType;
        private Type _queryableInterfaceType;
#if FEATURE_ISET
        private Type _setInterfaceType;
#endif
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

        public bool IsReadOnlyCollection => EnumerableType == ReadOnlyCollectionType;

        private bool IsEnumerableInterface => EnumerableType == EnumerableInterfaceType;

        public bool IsQueryableInterface => EnumerableType == QueryableInterfaceType;

        public bool IsEnumerableOrQueryable => IsEnumerableInterface || IsQueryableInterface;

        public bool HasCollectionInterface => EnumerableType.IsAssignableTo(CollectionInterfaceType);

#if FEATURE_ISET
        private bool HasSetInterface => EnumerableType.IsAssignableTo(SetInterfaceType);
#else
        private bool HasSetInterface => EnumerableType == HashSetType;
#endif
        public bool IsReadOnly => IsArray || IsReadOnlyCollection;

        public bool IsDeclaredReadOnly
            => IsReadOnly || IsEnumerableOrQueryable || IsReadOnlyCollectionInterface();

        public bool CouldBeReadOnly()
        {
            if (_couldBeReadOnly.HasValue)
            {
                return _couldBeReadOnly.Value;
            }

            if (EnumerableType.IsInterface())
            {
                // If the declared Type is an interface it could have an 'Add' method
                // while the underlying, implementing Type is actually readonly:
                return (_couldBeReadOnly = true).Value;
            }

            // If the declared Type declares an 'Add' method, assume it's not readonly;
            // Array implements ICollection<>, but its Add method is implemented explicitly:
            return (_couldBeReadOnly = EnumerableType.GetPublicInstanceMethods("Add").None()).Value;
        }

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

        public Type HashSetType => GetEnumerableType(ref _hashSetType, typeof(HashSet<>));

        public Type ReadOnlyCollectionType => GetEnumerableType(ref _readOnlyCollectionType, typeof(ReadOnlyCollection<>));

        public Type CollectionInterfaceType => GetEnumerableType(ref _collectionInterfaceType, typeof(ICollection<>));

        public Type EnumerableInterfaceType => GetEnumerableType(ref _enumerableInterfaceType, typeof(IEnumerable<>));

        public Type QueryableInterfaceType => GetEnumerableType(ref _queryableInterfaceType, typeof(IQueryable<>));

#if FEATURE_ISET
        private Type SetInterfaceType => GetEnumerableType(ref _setInterfaceType, typeof(ISet<>));
#endif
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

        public Expression GetCopyIntoObjectConstruction(Expression targetObject)
        {
            var objectType = HasSetInterface ? HashSetType : ListType;

            return Expression.New(
                objectType.GetPublicInstanceConstructor(EnumerableInterfaceType),
                targetObject);
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
                return instance.GetReadOnlyCollectionCreation(ElementType);
            }

            if (EnumerableType.IsAssignableTo(CollectionType))
            {
                return instance.GetCollectionTypeCreation(ElementType);
            }

            if (HasSetInterface)
            {
                return GetCopyIntoObjectConstruction(instance);
            }

            return instance.WithToListLinqCall(ElementType);
        }

        private static bool ValueIsNotEnumerableInterface(Expression instance)
            => instance.Type != typeof(IEnumerable<>).MakeGenericType(instance.Type.GetEnumerableElementType());

        public Expression GetCountFor(Expression instance, Type countType = null)
            => instance.GetCount(countType, exp => CollectionInterfaceType);

        public Expression GetNonZeroCountCheck(Expression enumerableAccess)
        {
            var enumerableCount = GetCountFor(enumerableAccess);
            var zero = ToNumericConverter<int>.Zero.GetConversionTo(enumerableCount.Type);
            var countGreaterThanZero = Expression.GreaterThan(enumerableCount, zero);

            return countGreaterThanZero;
        }

        public Type GetEmptyInstanceCreationFallbackType()
        {
            if (IsArray)
            {
                return ListType;
            }

            if (!EnumerableType.IsInterface())
            {
                return EnumerableType;
            }

            if (IsDictionary)
            {
                return typeof(Dictionary<,>).MakeGenericType(EnumerableType.GetGenericTypeArguments());
            }

            return HasSetInterface ? HashSetType : ListType;
        }
    }
}