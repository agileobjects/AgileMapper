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
    }

    internal class ObjectMappingCommand
    {
        public static IObjectMappingCommand<TInstance> CreateForChild<TSource, TTarget, TInstance>(
            TSource source,
            IQualifiedMember sourceMember,
            TTarget target,
            QualifiedMember targetMember,
            TInstance existingTargetInstance,
            QualifiedMember existingTargetInstanceMember,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            var command = Create(
                source,
                sourceMember,
                target,
                targetMember,
                existingTargetInstance,
                existingTargetInstanceMember,
                enumerableIndex,
                mappingContext);

            if (command.SourceMember.Matches(sourceMember))
            {
                return command;
            }

            var sourceParameter = Parameters.Create<TSource>("source");
            var targetParameter = Parameters.Create<TTarget>("target");
            var existingTargetInstanceParameter = Parameters.Create<TInstance>("existingTargetInstance");

            var sourceMemberAccess = command.SourceMember.GetAccess(sourceParameter);

            var existingTargetInstanceMemberParameter = Parameters.Create<QualifiedMember>("existingTargetInstanceMember");

            var commandType = typeof(ObjectMappingCommand<,,>).MakeGenericType(
                command.SourceMember.Type,
                command.TargetMember.Type,
                command.ExistingTargetInstanceMember.Type);

            var newObjectCall = Expression.New(
                commandType.GetConstructors().First(),
                sourceMemberAccess.GetConversionTo(command.SourceMember.Type),
                Parameters.SourceMember,
                targetParameter.GetConversionTo(command.TargetMember.Type),
                Parameters.TargetMember,
                existingTargetInstanceParameter.GetConversionTo(command.ExistingTargetInstanceMember.Type),
                existingTargetInstanceMemberParameter,
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var factoryLambda = Expression.Lambda<Func<
                TSource,
                IQualifiedMember,
                TTarget,
                QualifiedMember,
                TInstance,
                QualifiedMember,
                int?,
                MappingContext,
                IObjectMappingCommand<TInstance>>>(
                newObjectCall,
                sourceParameter,
                Parameters.SourceMember,
                targetParameter,
                Parameters.TargetMember,
                existingTargetInstanceParameter,
                existingTargetInstanceMemberParameter,
                Parameters.EnumerableIndexNullable,
                Parameters.MappingContext);

            var factory = factoryLambda.Compile();

            return factory.Invoke(
                command.Source,
                command.SourceMember,
                command.Target,
                command.TargetMember,
                command.ExistingTargetInstance,
                command.ExistingTargetInstanceMember,
                command.EnumerableIndex,
                command.MappingContext);
        }

        public static ObjectMappingCommand<TSource, TTarget, TInstance> Create<TSource, TTarget, TInstance>(
            TSource source,
            IQualifiedMember sourceMember,
            TTarget target,
            QualifiedMember targetMember,
            TInstance existingTargetInstance,
            QualifiedMember existingTargetInstanceMember,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            sourceMember = sourceMember.WithType(source.GetRuntimeSourceType());

            var targetMemberData = GetTargetData(sourceMember, targetMember, target, mappingContext);

            var instanceMemberData = (existingTargetInstanceMember != targetMember)
                ? GetTargetData(sourceMember, existingTargetInstanceMember, existingTargetInstance, mappingContext)
                : targetMemberData;

            return new ObjectMappingCommand<TSource, TTarget, TInstance>(
                source,
                instanceMemberData.SourceMember,
                target,
                targetMemberData.TargetMember,
                existingTargetInstance,
                instanceMemberData.TargetMember,
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

    internal class ObjectMappingCommand<TSource, TTarget, TInstance> : IObjectMappingCommand<TInstance>
    {
        public ObjectMappingCommand(
            TSource source,
            IQualifiedMember sourceMember,
            TTarget target,
            QualifiedMember targetMember,
            TInstance existingTargetInstance,
            QualifiedMember existingTargetInstanceMember,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            Source = source;
            SourceMember = sourceMember;

            Target = target;
            TargetMember = targetMember;

            ExistingTargetInstance = existingTargetInstance;
            ExistingTargetInstanceMember = existingTargetInstanceMember;

            EnumerableIndex = enumerableIndex;
            MappingContext = mappingContext;
        }

        public TSource Source { get; }

        public IQualifiedMember SourceMember { get; }

        public TTarget Target { get; }

        public QualifiedMember TargetMember { get; }

        public TInstance ExistingTargetInstance { get; }

        public QualifiedMember ExistingTargetInstanceMember { get; }

        public int? EnumerableIndex { get; }

        public MappingContext MappingContext { get; }

        public TInstance Execute() => MappingContext.MapChild(this);
    }
}