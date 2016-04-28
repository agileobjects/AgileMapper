namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class EnumerableMappingLambdaFactory<TSource, TTarget>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget> Instance =
            new EnumerableMappingLambdaFactory<TSource, TTarget>();

        protected override Expression GetObjectResolution(IObjectMappingContext omc)
        {
            var targetElementType = omc.TargetMember.ElementType;
            var listType = typeof(List<>).MakeGenericType(targetElementType);

            var value = listType.IsAssignableFrom(omc.ExistingObject.Type)
                ? Expression.Coalesce(omc.ExistingObject, Expression.New(listType))
                : GetNewListCreation(listType, targetElementType, omc);

            return value;
        }

        private static Expression GetNewListCreation(
            Type listType,
            Type targetElementType,
            IObjectMappingContext omc)
        {
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(targetElementType);
            var listConstructor = listType.GetConstructor(new[] { enumerableType });

            var typedEmptyEnumerableMethod = typeof(Enumerable)
                .GetMethod("Empty", Constants.PublicStatic)
                .MakeGenericMethod(targetElementType);

            var existingEnumerableOrEmpty = Expression.Coalesce(
                omc.ExistingObject,
                Expression.Call(typedEmptyEnumerableMethod));

            // ReSharper disable once AssignNullToNotNullAttribute
            return Expression.New(listConstructor, existingEnumerableOrEmpty);
        }

        protected override IEnumerable<Expression> GetObjectPopulation(IObjectMappingContext omc)
        {
            var sourceElementType = omc.SourceObject.Type.GetEnumerableElementType();
            var sourceElementParameter = Expression.Parameter(sourceElementType, "s");
            var targetElementType = omc.TargetMember.ElementType;
            var targetElementParameter = Expression.Parameter(targetElementType, "t");

            var selectMethod = typeof(Enumerable)
                .GetMethods(Constants.PublicStatic)
                .Last(m => m.Name == "Select")
                .MakeGenericMethod(sourceElementType, targetElementType);

            var mapCall = omc.GetMapCall(sourceElementParameter, Expression.Default(targetElementType));

            var selectFuncLambda = Expression.Lambda(
                Expression.GetFuncType(sourceElementType, typeof(int), targetElementType),
                mapCall,
                sourceElementParameter,
                Parameters.EnumerableIndex);

            var selectCall = Expression.Call(
                selectMethod,
                omc.SourceObject,
                selectFuncLambda);

            var forEachMethod = typeof(EnumerableExtensions)
                .GetMethod("ForEach", Constants.PublicStatic)
                .MakeGenericMethod(targetElementType);

            var addCall = Expression.Call(
                omc.TargetVariable,
                omc.TargetVariable.Type.GetMethod("Add", Constants.PublicInstance),
                targetElementParameter);

            var forEachActionLambda = Expression.Lambda(
                Expression.GetActionType(targetElementType, typeof(int)),
                addCall,
                targetElementParameter,
                Parameters.EnumerableIndex);

            var forEachCall = Expression.Call(
                forEachMethod,
                selectCall,
                forEachActionLambda);

            yield return forEachCall;
        }
    }
}