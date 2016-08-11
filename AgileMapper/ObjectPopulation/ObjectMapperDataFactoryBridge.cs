namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions;
    using Members;

    internal interface IObjectMappingContextFactoryBridge
    {
        bool Matches(BasicMapperData data);

        ObjectMapperData ToMapperData();
    }

    internal class ObjectMapperDataFactoryBridge : IObjectMappingContextFactoryBridge
    {
        private ObjectMapperDataFactoryBridge(
            Type declaredSourceType,
            Type declaredTargetType,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            MappingContext mappingContext)
        {
            DeclaredSourceType = declaredSourceType;
            DeclaredTargetType = declaredTargetType;
            SourceMember = sourceMember;
            TargetMember = targetMember;
            MappingContext = mappingContext;
        }

        #region FactoryMethod

        public static ObjectMapperDataFactoryBridge Create<TSource, TTarget>(
            MappingInstanceData<TSource, TTarget> instanceData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember)
        {
            sourceMember = sourceMember.WithType(instanceData.Source.GetRuntimeSourceType());
            targetMember = GetTargetMember(instanceData, sourceMember, targetMember);

            return new ObjectMapperDataFactoryBridge(
                typeof(TSource),
                typeof(TTarget),
                sourceMember,
                targetMember,
                instanceData.MappingContext);
        }

        private static QualifiedMember GetTargetMember<TSource, TTarget>(
            MappingInstanceData<TSource, TTarget> instanceData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember)
        {
            var mappingData = new BasicMapperData(instanceData.MappingContext.RuleSet, sourceMember.Type, typeof(TTarget));

            var targetMemberType =
                instanceData.MappingContext.MapperContext.UserConfigurations.DerivedTypePairs.GetDerivedTypeOrNull(mappingData)
                    ?? instanceData.Target.GetRuntimeTargetType(sourceMember.Type);

            targetMember = targetMember.WithType(targetMemberType);
            return targetMember;
        }

        #endregion

        public MappingContext MappingContext { get; }

        public Type DeclaredSourceType { get; }

        public Type DeclaredTargetType { get; }

        public IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        public bool Matches(BasicMapperData data)
            => (data.SourceType == SourceMember.Type) && (data.TargetType == TargetMember.Type);

        public ObjectMapperData ToMapperData() => ObjectMapperDataFactory.Create(this);
    }
}