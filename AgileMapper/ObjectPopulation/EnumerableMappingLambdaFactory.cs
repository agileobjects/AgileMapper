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

        protected override IEnumerable<Expression> GetShortCircuitReturns(GotoExpression returnNull, IObjectMappingContext omc)
            => Enumerable.Empty<Expression>();

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

        protected override IEnumerable<Expression> GetObjectPopulation(Expression targetVariableValue, IObjectMappingContext omc)
        {
            yield return omc.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(targetVariableValue, omc);
        }

        protected override Expression GetReturnValue(Expression targetVariableValue, IObjectMappingContext omc)
        {
            if (omc.TargetMember.Type.IsAssignableFrom(omc.TargetVariable.Type))
            {
                return omc.TargetVariable;
            }

            return omc.TargetVariable.WithToArrayCall();
        }
    }
}