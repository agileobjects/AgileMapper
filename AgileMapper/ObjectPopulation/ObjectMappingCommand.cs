namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;
    using Members;

    internal interface IObjectMappingCommand<out T>
    {
        T Execute();

        IObjectMappingContext ToOmc();
    }

    internal class ObjectMappingCommand
    {
        public static IObjectMappingCommand<TTarget> CreateForChild<TSource, TTarget>(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            var command = Create(
                sourceMember,
                source,
                targetMember,
                target,
                enumerableIndex,
                mappingContext);

            if (command.SourceMember.Matches(sourceMember))
            {
                return command;
            }

            var sourceParameter = Parameters.Create<TSource>("source");
            var targetParameter = Parameters.Create<TTarget>("target");

            var sourceMemberAccess = command.SourceMember.GetQualifiedAccess(sourceParameter);

            var commandType = typeof(ObjectMappingCommand<,>)
                .MakeGenericType(command.SourceMember.Type, command.TargetMember.Type);

            var newObjectCall = Expression.New(
                commandType.GetConstructors().First(),
                Parameters.SourceMember,
                sourceMemberAccess.GetConversionTo(command.SourceMember.Type),
                Parameters.TargetMember,
                targetParameter.GetConversionTo(command.TargetMember.Type),
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var factoryLambda = Expression.Lambda<Func<
                IQualifiedMember,
                TSource,
                QualifiedMember,
                TTarget,
                int?,
                MappingContext,
                IObjectMappingCommand<TTarget>>>(
                newObjectCall,
                Parameters.SourceMember,
                sourceParameter,
                Parameters.TargetMember,
                targetParameter,
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var factory = factoryLambda.Compile();

            return factory.Invoke(
                command.SourceMember,
                command.Source,
                command.TargetMember,
                command.Target,
                command.EnumerableIndex,
                command.MappingContext);
        }

        public static ObjectMappingCommand<TSource, TTarget> Create<TSource, TTarget>(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            sourceMember = sourceMember.WithType(source.GetRuntimeSourceType());

            var targetMemberData = GetTargetData(sourceMember, targetMember, target, mappingContext);

            return new ObjectMappingCommand<TSource, TTarget>(
                targetMemberData.SourceMember,
                source,
                targetMemberData.TargetMember,
                target,
                enumerableIndex,
                mappingContext);
        }

        private static MemberPair GetTargetData<TTarget>(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            TTarget target,
            MappingContext mappingContext)
        {
            var mappingData = new BasicMappingData(mappingContext.RuleSet, sourceMember.Type, typeof(TTarget));

            var bestMatchingSourceMember = GetBestMatchingSourceMember(sourceMember, targetMember, mappingContext);

            if (bestMatchingSourceMember != sourceMember)
            {
                mappingData = new BasicMappingData(mappingContext.RuleSet, bestMatchingSourceMember.Type, typeof(TTarget), mappingData);
            }

            var targetMemberType = mappingContext.MapperContext.UserConfigurations.GetDerivedTypeOrNull(mappingData)
                ?? target.GetRuntimeTargetType(bestMatchingSourceMember.Type);

            targetMember = targetMember.WithType(targetMemberType);

            return new MemberPair(
                targetMember.IsEnumerable ? bestMatchingSourceMember : sourceMember,
                targetMember);
        }

        private static IQualifiedMember GetBestMatchingSourceMember(
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            MappingContext mappingContext)
        {
            if ((mappingContext.CurrentObjectMappingContext == null) || sourceMember.Matches(targetMember))
            {
                return sourceMember;
            }

            var memberMappingContext = new MemberMappingContext(targetMember, mappingContext.CurrentObjectMappingContext);
            var matchingSourceMember = mappingContext.MapperContext.DataSources.GetSourceMemberFor(memberMappingContext);

            if (matchingSourceMember == null)
            {
                return sourceMember;
            }

            var matchingSourceMemberType = mappingContext
                .CurrentObjectMappingContext
                .GetSourceMemberRuntimeType(matchingSourceMember);

            return matchingSourceMember.WithType(matchingSourceMemberType);
        }

        #region Helper Classes

        private class MemberPair
        {
            public MemberPair(IQualifiedMember sourceMember, QualifiedMember targetMember)
            {
                SourceMember = sourceMember;
                TargetMember = targetMember;
            }

            public IQualifiedMember SourceMember { get; }

            public QualifiedMember TargetMember { get; }
        }

        private class BasicMappingData : IMappingData
        {
            public BasicMappingData(
                MappingRuleSet ruleSet,
                Type sourceType,
                Type targetType,
                IMappingData parent = null)
            {
                Parent = parent;
                SourceType = sourceType;
                TargetType = targetType;
                RuleSetName = ruleSet.Name;
            }

            public IMappingData Parent { get; }

            public string RuleSetName { get; }

            public Type SourceType { get; }

            public Type TargetType { get; }

            public QualifiedMember TargetMember => QualifiedMember.All;
        }

        #endregion
    }

    internal class ObjectMappingCommand<TSource, TTarget> : IObjectMappingCommand<TTarget>
    {
        public ObjectMappingCommand(
            IQualifiedMember sourceMember,
            TSource source,
            QualifiedMember targetMember,
            TTarget target,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            Source = source;
            SourceMember = sourceMember;

            Target = target;
            TargetMember = targetMember;

            EnumerableIndex = enumerableIndex;
            MappingContext = mappingContext;
        }

        public TSource Source { get; }

        public IQualifiedMember SourceMember { get; }

        public TTarget Target { get; }

        public QualifiedMember TargetMember { get; }

        public int? EnumerableIndex { get; }

        public MappingContext MappingContext { get; }

        public TTarget Execute() => MappingContext.MapChild(this);

        public IObjectMappingContext ToOmc() => ObjectMappingContextFactory.Create(this);
    }
}