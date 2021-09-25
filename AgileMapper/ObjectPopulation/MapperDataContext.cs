namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal class MapperDataContext
    {
        private readonly ObjectMapperData _mapperData;
        private bool _usesMappingDataObjectAsParameter;
        private bool? _isMappingDataObjectNeeded;

        public MapperDataContext(IMemberMapperData childMapperData)
            : this(
                childMapperData.Parent,
                RuntimeTypesNeeded(childMapperData),
                childMapperData.Parent.Context.IsForDerivedType,
                childMapperData)
        {
        }

        #region Setup

        private static bool RuntimeTypesNeeded(ITypePair mapperData)
            => mapperData.SourceType.RuntimeTypeNeeded() || mapperData.TargetType.RuntimeTypeNeeded();

        #endregion

        public MapperDataContext(ObjectMapperData mapperData, bool isStandalone, bool isForDerivedType)
            : this(mapperData, isStandalone, isForDerivedType, mapperData)
        {
        }

        private MapperDataContext(
            ObjectMapperData mapperData,
            bool isStandalone,
            bool isForDerivedType,
            IQualifiedMemberContext context)
        {
            _mapperData = mapperData;
            IsStandalone = isStandalone;
            IsForDerivedType = isForDerivedType;
            UseLocalTargetVariable = isForDerivedType || ShouldUseLocalVariable(context);
        }

        #region Setup

        private static bool ShouldUseLocalVariable(IQualifiedMemberContext context)
        {
            if (context.TargetMember.IsSimple &&
               !context.TargetType.GetNonNullableType().IsEnum())
            {
                return false;
            }

            if (context.UseSingleMappingExpression())
            {
                return false;
            }

            if (context.TargetMember.IsComplex &&
               (context.TargetMember.IsReadOnly || context.TargetIsDefinitelyPopulated()) &&
               !context.TargetMemberIsUserStruct())
            {
                return false;
            }

            return true;
        }

        #endregion

        public bool IsStandalone { get; }

        public bool IsForDerivedType { get; }

        public bool IsForToTargetMapping { get; set; }

        public bool IsForNewElement { get; set; }

        public bool NeedsRuntimeTypedMapping { get; private set; }

        public void RuntimeTypedMappingNeeded()
        {
            if (NeedsRuntimeTypedMapping)
            {
                return;
            }

            NeedsRuntimeTypedMapping = true;
            BubbleRuntimeTypedMappingNeededToEntryPoint();
        }

        private void BubbleRuntimeTypedMappingNeededToEntryPoint()
        {
            if (!_mapperData.IsEntryPoint)
            {
                _mapperData.Parent.Context.RuntimeTypedMappingNeeded();
            }
        }

        public bool UseLocalTargetVariable { get; }

        public bool UseMappingTryCatch
            => _mapperData.RuleSet.Settings.UseTryCatch && (_mapperData.IsEntryPoint || !IsPartOfUserStructMapping());

        public bool IsPartOfUserStructMapping()
            => CheckHierarchy(mapperData => mapperData.TargetMemberIsUserStruct());

        public bool IsPartOfQueryableMapping()
            => CheckHierarchy(mapperData => mapperData.SourceType.IsQueryable());

        private bool CheckHierarchy(Func<IQualifiedMemberContext, bool> predicate)
        {
            var mapperData = _mapperData;

            while (mapperData != null)
            {
                if (predicate.Invoke(mapperData))
                {
                    return true;
                }

                mapperData = mapperData.Parent;
            }

            return false;
        }

        public bool UsesMappingDataObjectAsParameter
        {
            get
            {
                return _mapperData.Context._usesMappingDataObjectAsParameter ||
                       _mapperData.AnyChildMapperDataMatches(cmd => cmd.Context.UsesMappingDataObjectAsParameter);
            }
            set
            {
                _mapperData.Context._usesMappingDataObjectAsParameter =
                    _mapperData.Context._usesMappingDataObjectAsParameter || value;
            }
        }

        public bool UsesMappingDataObject
        {
            get
            {
                return (_isMappingDataObjectNeeded ??=
                        (NeedsRuntimeTypedMapping || UsesMappingDataObjectAsParameter ||
                        _mapperData.AnyChildMapperDataMatches(cmd => cmd.Context.UsesMappingDataObject)));
            }
        }

        public bool Compile => IsStandalone && !IsForDerivedType;
    }
}