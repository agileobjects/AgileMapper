namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions;
    using Members;

    internal static class ObjectMapperDataBridge
    {
        public static ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget> Create<TDeclaredSource, TDeclaredTarget>(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> instanceData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData objectMapperData = null)
        {
            if (CheckSourceRuntimeType(sourceMember))
            {
                sourceMember = sourceMember.WithType(instanceData.Source.GetRuntimeSourceType());
            }

            if (CheckTargetRuntimeType(targetMember))
            {
                targetMember = GetTargetMember(instanceData, sourceMember, targetMember);
            }

            return new ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget>(
                instanceData,
                objectMapperData,
                typeof(TDeclaredSource),
                typeof(TDeclaredTarget),
                sourceMember,
                targetMember);
        }

        private static bool CheckSourceRuntimeType(IQualifiedMember sourceMember)
        {
            if (sourceMember.IsEnumerable)
            {
                return !sourceMember.Type.IsGenericType;
            }

            return !sourceMember.Type.IsSealed;
        }

        private static bool CheckTargetRuntimeType(IQualifiedMember targetMember)
        {
            if (targetMember.IsEnumerable)
            {
                return targetMember.Type.IsInterface;
            }

            return !targetMember.Type.IsSealed;
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

            return targetMember.WithType(targetMemberType);
        }
    }

    internal class ObjectMapperDataBridge<TDeclaredSource, TDeclaredTarget> : IObjectMapperDataBridge
    {
        private readonly ObjectMapperData _mapperData;

        public ObjectMapperDataBridge(
            MappingInstanceData<TDeclaredSource, TDeclaredTarget> instanceData,
            ObjectMapperData mapperData,
            Type declaredSourceType,
            Type declaredTargetType,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember)
        {
            InstanceData = instanceData;
            _mapperData = mapperData;
            DeclaredSourceType = declaredSourceType;
            DeclaredTargetType = declaredTargetType;
            SourceMember = sourceMember;
            TargetMember = targetMember;

            RuntimeTypesAreTheSame = (SourceMember.Type == DeclaredSourceType) && (TargetMember.Type == DeclaredTargetType);
        }

        public MappingContext MappingContext => InstanceData.MappingContext;

        public MappingInstanceData<TDeclaredSource, TDeclaredTarget> InstanceData { get; }

        public Type DeclaredSourceType { get; }

        public Type DeclaredTargetType { get; }

        public IQualifiedMember SourceMember { get; }

        public QualifiedMember TargetMember { get; }

        public bool RuntimeTypesAreTheSame { get; }

        public ObjectMapperData GetMapperData(ObjectMapperKey key)
        {
            return new ObjectMapperData(
                MappingContext,
                SourceMember,
                TargetMember,
                RuntimeTypesAreTheSame,
                key,
                _mapperData);
        }

        public IObjectMapperCreationData GetCreationData() => MapperCreationDataFactory.Create(this);
    }
}