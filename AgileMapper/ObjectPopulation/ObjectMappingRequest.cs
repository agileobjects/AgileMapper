namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Extensions;
    using Members;

    internal class ObjectMappingRequest<TDeclaredSource, TDeclaredTarget, TDeclaredInstance>
    {
        public ObjectMappingRequest(
            TDeclaredSource source,
            IQualifiedMember sourceMember,
            TDeclaredTarget target,
            IQualifiedMember targetMember,
            TDeclaredInstance existingTargetInstance,
            IQualifiedMember existingTargetInstanceMember,
            int? enumerableIndex,
            MappingContext mappingContext)
        {
            Source = source;
            SourceMember = sourceMember.WithType(source.GetRuntimeSourceType());
            Target = target;
            TargetMember = targetMember.WithType(target.GetRuntimeTargetType(SourceMember.Type));
            ExistingTargetInstance = existingTargetInstance;
            ExistingTargetInstanceMember = existingTargetInstanceMember.WithType(existingTargetInstance.GetRuntimeTargetType(SourceMember.Type));
            EnumerableIndex = enumerableIndex ?? mappingContext.CurrentObjectMappingContext?.GetEnumerableIndex();
            MappingContext = mappingContext;
        }

        public TDeclaredSource Source { get; }

        public IQualifiedMember SourceMember { get; }

        public TDeclaredTarget Target { get; }

        public IQualifiedMember TargetMember { get; }

        public TDeclaredInstance ExistingTargetInstance { get; }

        public IQualifiedMember ExistingTargetInstanceMember { get; }

        public int? EnumerableIndex { get; }

        public MappingContext MappingContext { get; }
    }
}