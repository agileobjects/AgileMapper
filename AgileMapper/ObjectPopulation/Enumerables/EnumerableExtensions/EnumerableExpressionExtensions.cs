namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables.EnumerableExtensions
{
    using System;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using static Extensions.Internal.LinqExtensions;

    internal static class EnumerableExpressionExtensions
    {
        private static readonly MethodInfo _listToArrayMethod;
        private static readonly MethodInfo _collectionToArrayMethod;

        static EnumerableExpressionExtensions()
        {
            var toArrayExtensionMethods = typeof(PublicEnumerableExtensions).GetPublicStaticMethods("ToArray").ToArray();
            _listToArrayMethod = toArrayExtensionMethods.First();
            _collectionToArrayMethod = toArrayExtensionMethods.Last();
        }

        public static Expression WithToArrayCall(this Expression enumerable, Type elementType)
        {
            var conversionMethod = GetToArrayConversionMethod(enumerable, elementType);

            return enumerable.GetToEnumerableCall(conversionMethod, elementType);
        }

        private static MethodInfo GetToArrayConversionMethod(Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (TryGetWrapperMethod(typeHelper, "ToArray", out var method))
            {
                return method;
            }

            if (typeHelper.HasListInterface)
            {
                return _listToArrayMethod;
            }

            return GetNonListToArrayConversionMethod(typeHelper);
        }

        public static Expression GetEmptyInstanceCreation(
            this Type enumerableType,
            Type elementType,
            EnumerableTypeHelper typeHelper = null)
        {
            if (enumerableType.IsArray)
            {
                return GetEmptyArray(elementType);
            }

            if (enumerableType.IsValueType())
            {
                return Expression.New(enumerableType);
            }

            if (typeHelper == null)
            {
                typeHelper = new EnumerableTypeHelper(enumerableType, elementType);
            }

            if (typeHelper.IsEnumerableOrQueryable)
            {
                return Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "Empty");
            }

            if (typeHelper.IsReadOnlyCollection)
            {
                return GetReadOnlyCollectionCreation(typeHelper, GetEmptyArray(elementType));
            }

            return Expression.New(typeHelper.GetEmptyInstanceCreationFallbackType());
        }

        private static Expression GetEmptyArray(Type elementType)
             => Expression.Field(null, typeof(Enumerable<>).MakeGenericType(elementType), "EmptyArray");

        public static Expression GetReadOnlyCollectionCreation(this Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (TryGetWrapperMethod(typeHelper, "ToReadOnlyCollection", out var method))
            {
                return enumerable.GetToEnumerableCall(method, typeHelper.ElementType);
            }

            if (typeHelper.IsList)
            {
                return Expression.Call(enumerable, typeHelper.ListType.GetPublicInstanceMethod("AsReadOnly"));
            }

            if (typeHelper.HasListInterface)
            {
                return GetReadOnlyCollectionCreation(typeHelper, enumerable);
            }

            var nonListToArrayMethod = GetNonListToArrayConversionMethod(typeHelper);
            var toArrayCall = enumerable.GetToEnumerableCall(nonListToArrayMethod, typeHelper.ElementType);

            return GetReadOnlyCollectionCreation(typeHelper, toArrayCall);
        }

        private static Expression GetReadOnlyCollectionCreation(EnumerableTypeHelper typeHelper, Expression list)
            => GetCopyListCollectionCreation(typeHelper, typeHelper.ReadOnlyCollectionType, list);

        public static Expression GetCollectionTypeCreation(this Expression enumerable, Type elementType)
        {
            var typeHelper = new EnumerableTypeHelper(enumerable.Type, elementType);

            if (typeHelper.HasListInterface)
            {
                return GetCollectionTypeCreation(typeHelper, enumerable.GetConversionTo(typeHelper.ListInterfaceType));
            }

            var nonListToArrayMethod = GetNonListToArrayConversionMethod(typeHelper);
            var toArrayCall = enumerable.GetToEnumerableCall(nonListToArrayMethod, typeHelper.ElementType);

            return GetCollectionTypeCreation(typeHelper, toArrayCall);
        }

        private static Expression GetCollectionTypeCreation(EnumerableTypeHelper typeHelper, Expression list)
            => GetCopyListCollectionCreation(typeHelper, typeHelper.CollectionType, list);

        private static Expression GetCopyListCollectionCreation(
            EnumerableTypeHelper typeHelper,
            Type collectonType,
            Expression list)
        {
            var copyListConstructor = collectonType.GetPublicInstanceConstructor(typeHelper.ListInterfaceType);

            return (copyListConstructor != null)
                ? Expression.New(copyListConstructor, list)
                : Expression.New(collectonType);
        }

        private static bool TryGetWrapperMethod(
            EnumerableTypeHelper typeHelper,
            string methodName,
            out MethodInfo method)
        {
            var wrapperType = typeHelper.WrapperType;

            if (typeHelper.EnumerableType != wrapperType)
            {
                method = null;
                return false;
            }

            method = wrapperType.GetPublicInstanceMethod(methodName);
            return true;
        }

        private static MethodInfo GetNonListToArrayConversionMethod(EnumerableTypeHelper typeHelper)
            => typeHelper.HasCollectionInterface ? _collectionToArrayMethod : LinqToArrayMethod;
    }
}
