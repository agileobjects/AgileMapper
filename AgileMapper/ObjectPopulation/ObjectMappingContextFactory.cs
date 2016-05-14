namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var runtimeTypes = GetRuntimeTypes(source, target);
            var sourceMember = QualifiedMember.From(Member.RootSource(runtimeTypes.Item1));
            var targetMember = QualifiedMember.From(Member.RootTarget(runtimeTypes.Item2));

            return Create(source, target, target, runtimeTypes, sourceMember, targetMember, mappingContext);
        }

        public static IObjectMappingContext Create<TRuntimeSource, TRuntimeTarget, TDeclaredMember>(
            TRuntimeSource source,
            TRuntimeTarget target,
            QualifiedMember childTargetMember,
            TDeclaredMember childMemberValue,
            MappingContext mappingContext)
        {
            var targetMemberRuntimeType = target != null
                ? childMemberValue.GetRuntimeTargetType(typeof(TRuntimeSource))
                : typeof(TDeclaredMember);

            QualifiedMember qualifiedSourceMember;
            Type sourceMemberRuntimeType;

            if (mappingContext.CurrentObjectMappingContext.HasSource(source))
            {
                qualifiedSourceMember = mappingContext.CurrentObjectMappingContext.SourceMember;
                sourceMemberRuntimeType = qualifiedSourceMember.Type;
            }
            else
            {
                var childMemberContext = new MemberMappingContext(childTargetMember, mappingContext.CurrentObjectMappingContext);

                qualifiedSourceMember = mappingContext
                    .MapperContext
                    .DataSources
                    .GetSourceMemberMatching(childMemberContext);

                sourceMemberRuntimeType = source.GetRuntimeSourceType();
                qualifiedSourceMember = qualifiedSourceMember.WithType(sourceMemberRuntimeType);
            }

            var runtimeTypes = Tuple.Create(
                sourceMemberRuntimeType,
                typeof(TRuntimeTarget),
                targetMemberRuntimeType);

            return Create(
                source,
                target,
                childMemberValue,
                runtimeTypes,
                qualifiedSourceMember,
                childTargetMember,
                mappingContext);
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
                existingElement,
                runtimeTypes,
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.SourceMember, runtimeTypes.Item1),
                GetEnumerableElementMember(mappingContext.CurrentObjectMappingContext.TargetMember, runtimeTypes.Item2),
                mappingContext,
                enumerableIndex);
        }

        private static QualifiedMember GetEnumerableElementMember(QualifiedMember enumerableMember, Type runtimeType)
            => enumerableMember.Append(enumerableMember.Type.CreateElementMember().WithType(runtimeType));

        private static Tuple<Type, Type, Type> GetRuntimeTypes<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget existing)
        {
            var sourceType = source.GetRuntimeSourceType();
            var targetType = existing.GetRuntimeTargetType(sourceType);

            return Tuple.Create(sourceType, targetType, targetType);
        }

        private delegate IObjectMappingContext ObjectMappingContextCreator<in TSource, in TTarget, in TInstance>(
            QualifiedMember sourceMember,
            QualifiedMember targetMember,
            TSource source,
            TTarget target,
            TInstance existing,
            int? enumerableIndex,
            MappingContext mappingContext);

        private static IObjectMappingContext Create<TSource, TTarget, TInstance>(
            TSource source,
            TTarget target,
            TInstance existing,
            Tuple<Type, Type, Type> runtimeTypes,
            QualifiedMember sourceMember,
            QualifiedMember targetMember,
            MappingContext mappingContext,
            int? enumerableIndex = null)
        {
            var funcKey = string.Format(
                CultureInfo.InvariantCulture,
                "{0}({1}),{2}({3}),{4}({5}): ObjectMappingContextConstructor",
                typeof(TSource).FullName,
                runtimeTypes.Item1,
                typeof(TTarget).FullName,
                runtimeTypes.Item2,
                typeof(TInstance).FullName,
                runtimeTypes.Item3);

            var constructionFunc = mappingContext.GlobalContext.Cache.GetOrAdd(funcKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var targetParameter = Parameters.Create<TTarget>("target");
                var existingInstanceParameter = Parameters.Create<TInstance>("existingInstance");

                var contextType = typeof(ObjectMappingContext<,,>)
                    .MakeGenericType(runtimeTypes.Item1, runtimeTypes.Item2, runtimeTypes.Item3);

                var constructorCall = Expression.New(
                    contextType.GetConstructors().First(),
                    Parameters.SourceMember,
                    Parameters.TargetMember,
                    sourceParameter.GetConversionTo(runtimeTypes.Item1),
                    targetParameter.GetConversionTo(runtimeTypes.Item2),
                    existingInstanceParameter.GetConversionTo(runtimeTypes.Item3),
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

                var constructionLambda = Expression
                    .Lambda<ObjectMappingContextCreator<TSource, TTarget, TInstance>>(
                        constructorCall,
                        Parameters.SourceMember,
                        Parameters.TargetMember,
                        sourceParameter,
                        targetParameter,
                        existingInstanceParameter,
                        Parameters.EnumerableIndexNullable,
                        Parameters.MappingContext);

                return constructionLambda.Compile();
            });

            if (enumerableIndex == null)
            {
                enumerableIndex = mappingContext.CurrentObjectMappingContext?.GetEnumerableIndex();
            }

            return constructionFunc.Invoke(sourceMember, targetMember, source, target, existing, enumerableIndex, mappingContext);
        }
    }
}