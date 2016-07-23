namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal static class ObjectMappingContextFactory
    {
        private delegate IObjectMappingContext ObjectMappingContextCreator<in TSource, in TTarget>(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext);

        public static IObjectMappingContext CreateRoot<TDeclaredSource, TDeclaredTarget>(
            TDeclaredSource source,
            TDeclaredTarget target,
            MappingContext mappingContext)
        {
            var sourceMember = QualifiedMember.From(Member.RootSource(typeof(TDeclaredSource)), mappingContext.MapperContext.NamingSettings);
            var targetMember = QualifiedMember.From(Member.RootTarget(typeof(TDeclaredTarget)), mappingContext.MapperContext.NamingSettings);

            return Create(ObjectMappingContextFactoryBridge.Create(
                sourceMember,
                source,
                targetMember,
                target,
                default(int?),
                mappingContext));
        }

        public static IObjectMappingContext Create<TSource, TTarget>(
            ObjectMappingContextFactoryBridge<TSource, TTarget> command)
        {
            var funcKey = OmcConstructorKey.From(command);

            var constructionFunc = command.MappingContext.GlobalContext.Cache.GetOrAdd(funcKey, k =>
            {
                var sourceParameter = Parameters.Create<TSource>("source");
                var targetParameter = Parameters.Create<TTarget>("target");

                var contextType = typeof(ObjectMappingContext<,>)
                    .MakeGenericType(command.SourceMember.Type, command.TargetMember.Type);

                var constructorCall = Expression.New(
                    contextType.GetConstructors().First(),
                    Parameters.SourceMember,
                    sourceParameter.GetConversionTo(command.SourceMember.Type),
                    Parameters.TargetMember,
                    targetParameter.GetConversionTo(command.TargetMember.Type),
                    Parameters.EnumerableIndexNullable,
                    Parameters.MappingContext);

                var constructionLambda = Expression
                    .Lambda<ObjectMappingContextCreator<TSource, TTarget>>(
                        constructorCall,
                        Parameters.SourceMember,
                        sourceParameter,
                        Parameters.TargetMember,
                        targetParameter,
                        Parameters.EnumerableIndexNullable,
                        Parameters.MappingContext);

                return constructionLambda.Compile();
            });

            return constructionFunc.Invoke(
                command.SourceMember,
                command.Source,
                command.TargetMember,
                command.Target,
                command.EnumerableIndex,
                command.MappingContext);
        }

        private class OmcConstructorKey
        {
            private readonly Type _declaredSourceType;
            private readonly Type _runtimeSourceType;
            private readonly Type _declaredTargetType;
            private readonly Type _runtimeTargetType;
            private readonly bool _sourceTypesAreTheSame;
            private readonly bool _targetTypesAreTheSame;

            private OmcConstructorKey(
                Type declaredSourceType,
                Type runtimeSourceType,
                Type declaredTargetType,
                Type runtimeTargetType)
            {
                _declaredSourceType = declaredSourceType;
                _runtimeSourceType = runtimeSourceType;
                _sourceTypesAreTheSame = (declaredSourceType == runtimeSourceType);
                _declaredTargetType = declaredTargetType;
                _runtimeTargetType = runtimeTargetType;
                _targetTypesAreTheSame = (declaredTargetType == runtimeTargetType);
            }

            public static OmcConstructorKey From<TSource, TTarget>(
                ObjectMappingContextFactoryBridge<TSource, TTarget> command)
            {
                return new OmcConstructorKey(
                    typeof(TSource),
                    command.SourceMember.Type,
                    typeof(TTarget),
                    command.TargetMember.Type);
            }

            public override bool Equals(object obj)
            {
                var otherKey = obj as OmcConstructorKey;

                if (otherKey == null)
                {
                    return false;
                }

                return
                    otherKey._declaredSourceType == _declaredSourceType &&
                    ((_sourceTypesAreTheSame && otherKey._sourceTypesAreTheSame) || otherKey._runtimeSourceType == _runtimeSourceType) &&
                    otherKey._declaredTargetType == _declaredTargetType &&
                    ((_targetTypesAreTheSame && otherKey._targetTypesAreTheSame) || otherKey._runtimeTargetType == _runtimeTargetType);
            }

            public override int GetHashCode() => 0;
        }
    }
}