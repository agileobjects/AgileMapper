namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal class EnumerableMappingLambdaFactory<TSource, TTarget, TInstance>
        : ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance>
    {
        public static readonly ObjectMappingLambdaFactoryBase<TSource, TTarget, TInstance> Instance =
            new EnumerableMappingLambdaFactory<TSource, TTarget, TInstance>();

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

        protected override IEnumerable<Expression> GetObjectPopulation(Expression instanceVariableValue, IObjectMappingContext omc)
        {
            yield return omc.MappingContext.RuleSet.EnumerablePopulationStrategy.GetPopulation(instanceVariableValue, omc);
        }

        protected override Expression GetReturnValue(Expression instanceVariableValue, IObjectMappingContext omc)
        {
            if (omc.TargetMember.Type.IsAssignableFrom(omc.InstanceVariable.Type))
            {
                return omc.InstanceVariable;
            }

            return omc.InstanceVariable.WithToArrayCall();
        }
    }
}