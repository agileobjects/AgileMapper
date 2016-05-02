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
            var sourceMember = QualifiedMember.From(Member.RootSource(runtimeTypes.Item1));
            var targetMember = QualifiedMember.From(Member.RootTarget(runtimeTypes.Item2));

            return Create(source, existing, runtimeTypes, sourceMember, targetMember, mappingContext);
        }

        public static IObjectMappingContext Create<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            TRuntimeSource source,
            TRuntimeTarget existing,
            Expression<Func<TRuntimeTarget, TDeclaredMember>> childTargetMemberExpression,
            MappingContext mappingContext)
        {
            TDeclaredMember existingMemberValue;
            Type targetMemberRuntimeType;

            if (existing != null)
            {
                existingMemberValue = childTargetMemberExpression.Compile().Invoke(existing);
                targetMemberRuntimeType = GetRuntimeTargetType(existingMemberValue, typeof(TRuntimeSource));
            }
            else
            {
                existingMemberValue = default(TDeclaredMember);
                targetMemberRuntimeType = typeof(TDeclaredMember);
            }

            var childTargetMember = GetChildTargetMember(childTargetMemberExpression, targetMemberRuntimeType);

            var qualifiedChildTargetMember = mappingContext
                .CurrentObjectMappingContext
                .TargetMember
                .Append(childTargetMember);

            QualifiedMember qualifiedSourceMember;
            Type sourceMemberRuntimeType;

            if (mappingContext.CurrentObjectMappingContext.HasSource(source))
            {
                qualifiedSourceMember = mappingContext.CurrentObjectMappingContext.SourceMember;
                sourceMemberRuntimeType = qualifiedSourceMember.Type;
            }
            else
            {
                qualifiedSourceMember = mappingContext
                    .MapperContext
                    .DataSources
                    .GetSourceMemberMatching(qualifiedChildTargetMember, mappingContext.CurrentObjectMappingContext);

                sourceMemberRuntimeType = GetRuntimeSourceType(source);

                qualifiedSourceMember = qualifiedSourceMember.WithType(sourceMemberRuntimeType);
            }

            return Create(
                source,
                existingMemberValue,
                Tuple.Create(sourceMemberRuntimeType, targetMemberRuntimeType),
                qualifiedSourceMember,
                qualifiedChildTargetMember,
                mappingContext);
        }

        private static Member GetChildTargetMember<TRuntimeTarget, TDeclaredMember>(
            Expression<Func<TRuntimeTarget, TDeclaredMember>> childTargetMemberExpression,
            Type targetMemberRuntimeType)
        {
            var childTargetMemberInfo = ((MemberExpression)childTargetMemberExpression.Body).Member;
            var childTargetMemberType = (childTargetMemberInfo is PropertyInfo) ? MemberType.Property : MemberType.Field;

            return new Member(
                childTargetMemberType,
                childTargetMemberInfo.Name,
                typeof(TRuntimeTarget),
                targetMemberRuntimeType);
        }

        public static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource sourceElement,
            TDeclaredTarget existingElement,
            int enumerableIndex,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(sourceElement, existingElement);

            return Create(
                sourceElement,
                existingElement,
                runtimeTypes,
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.SourceMember, runtimeTypes.Item1),
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.TargetMember, runtimeTypes.Item2),
                mappingContext,
                enumerableIndex);
        }

        private static QualifiedMember GetEnumerableElementMember(QualifiedMember enumerableMember, Type runtimeType)
        {
            return enumerableMember.Append(enumerableMember.Type.CreateElementMember().WithType(runtimeType));
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

        private delegate IObjectMappingContext ObjectMappingContextCreator<in TDeclaredSource, in TDeclaredTarget>(
            QualifiedMember sourceMember,
            QualifiedMember targetMember,
            TDeclaredSource source,
            TDeclaredTarget existing,
            int? enumerableIndex,
            MappingContext mappingContext);

        private static IObjectMappingContext Create<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing,
            Tuple<Type, Type> runtimeTypes,
            QualifiedMember sourceMember,
            QualifiedMember targetMember,
            MappingContext mappingContext,
            int? enumerableIndex = null)
        {
            var sourceParameter = Expression.Parameter(typeof(TDeclaredSource), "source");
            var existingParameter = Expression.Parameter(typeof(TDeclaredTarget), "existing");

            var contextType = typeof(ObjectMappingContext<,>)
                .MakeGenericType(runtimeTypes.Item1, runtimeTypes.Item2);

            var constructorCall = Expression.New(
                contextType.GetConstructors().First(),
                Parameters.SourceMember,
                Parameters.TargetMember,
                sourceParameter.GetConversionTo(runtimeTypes.Item1),
                existingParameter.GetConversionTo(runtimeTypes.Item2),
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var constructionLambda = Expression
                .Lambda<ObjectMappingContextCreator<TDeclaredSource, TDeclaredTarget>>(
                    constructorCall,
                    Parameters.SourceMember,
                    Parameters.TargetMember,
                    sourceParameter,
                    existingParameter,
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

            var constructionFunc = constructionLambda.Compile();

            return constructionFunc.Invoke(sourceMember, targetMember, source, existing, enumerableIndex, mappingContext);
        }
    }
}