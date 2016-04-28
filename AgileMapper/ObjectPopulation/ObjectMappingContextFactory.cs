namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(source, existing);
            var targetMember = Member.RootTarget(runtimeTypes.Item2);

            return Create(source, existing, runtimeTypes, targetMember, mappingContext);
        }

        public static IObjectMappingContext Create<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            TRuntimeSource source,
            TRuntimeTarget existing,
            Expression<Func<TRuntimeTarget, TDeclaredMember>> childTargetMemberExpression,
            MappingContext mappingContext)
        {
            TDeclaredMember existingMemberValue;
            Type runtimeMemberType;

            if (existing != null)
            {
                existingMemberValue = childTargetMemberExpression.Compile().Invoke(existing);
                runtimeMemberType = GetRuntimeTargetType(existingMemberValue, typeof(TRuntimeSource));
            }
            else
            {
                existingMemberValue = default(TDeclaredMember);
                runtimeMemberType = typeof(TDeclaredMember);
            }

            var childMemberInfo = ((MemberExpression)childTargetMemberExpression.Body).Member;

            var childMemberType = (childMemberInfo is PropertyInfo)
                ? MemberType.Property
                : MemberType.Field;

            var childMember = new Member(childMemberType, childMemberInfo.Name, runtimeMemberType);

            var runtimeSourceType = childMember.IsComplex ? typeof(TRuntimeSource) : GetRuntimeSourceType(source);

            return Create(
                source,
                existingMemberValue,
                Tuple.Create(runtimeSourceType, runtimeMemberType),
                childMember,
                mappingContext);
        }

        public static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget existingElement,
            int enumerableIndex,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(sourceElement, existingElement);

            var targetMember = mappingContext
                .CurrentObjectMappingContext
                .TargetMember
                .ElementType
                .CreateElementMember();

            return Create(
                sourceElement,
                existingElement,
                runtimeTypes,
                targetMember,
                mappingContext,
                enumerableIndex);
        }

        private static Tuple<Type, Type> GetRuntimeTypes<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            var sourceType = GetRuntimeSourceType(source);
            var targetType = GetRuntimeTargetType(existing, sourceType);

            return Tuple.Create(sourceType, targetType);
        }

        private static Type GetRuntimeSourceType<TDeclaredSource>(TDeclaredSource source)
        {
            return typeof(TDeclaredSource).CouldHaveADifferentRuntimeType()
                ? source.GetType()
                : typeof(TDeclaredSource);
        }

        private static Type GetRuntimeTargetType<TDeclaredTarget>(TDeclaredTarget existing, Type sourceType)
        {
            return (existing != null)
                ? existing.GetType()
                : typeof(TDeclaredTarget).IsAssignableFrom(sourceType) ? sourceType : typeof(TDeclaredTarget);
        }

        private static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing,
            Tuple<Type, Type> runtimeTypes,
            Member targetMember,
            MappingContext mappingContext,
            int? enumerableIndex = null)
        {
            var sourceParameter = Expression.Parameter(typeof(TDeclaredSource), "source");
            var existingParameter = Expression.Parameter(typeof(TDeclaredTarget), "existing");

            var contextType = typeof(ObjectMappingContext<,>)
                .MakeGenericType(runtimeTypes.Item1, runtimeTypes.Item2);

            var constructorCall = Expression.New(
                contextType.GetConstructors().First(),
                Parameters.TargetMember,
                Expression.Convert(sourceParameter, runtimeTypes.Item1),
                Expression.Convert(existingParameter, runtimeTypes.Item2),
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var constructionLambda = Expression
                .Lambda<Func<Member, TDeclaredSource, TDeclaredTarget, int?, MappingContext, IObjectMappingContext>>(
                    constructorCall,
                    Parameters.TargetMember,
                    sourceParameter,
                    existingParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

            var constructionFunc = constructionLambda.Compile();

            return constructionFunc.Invoke(targetMember, source, existing, enumerableIndex, mappingContext);
        }
    }
}