namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using Extensions;
    using Members;
    using NetStandardPolyfills;

    internal class MapperDataContext
    {
        private readonly ObjectMapperData _mapperData;
        private bool _usesMappingDataObjectAsParameter;
        private bool? _isMappingDataObjectNeeded;

        public MapperDataContext(IMemberMapperData childMapperData)
        {
            _mapperData = childMapperData.Parent;
            IsStandalone = IsForStandaloneMapping(childMapperData);
            IsForDerivedType = _mapperData.Context.IsForDerivedType;
        }

        private static bool IsForStandaloneMapping(IBasicMapperData mapperData)
            => mapperData.SourceType.RuntimeTypeNeeded() || mapperData.TargetType.RuntimeTypeNeeded();

        public MapperDataContext(
            ObjectMapperData mapperData,
            bool isStandalone,
            bool isForDerivedType)
        {
            _mapperData = mapperData;
            IsStandalone = isStandalone;
            IsForDerivedType = isForDerivedType;
            UseLocalVariable = isForDerivedType || ShouldUseLocalVariable(mapperData);
        }

        private static bool ShouldUseLocalVariable(IMemberMapperData mapperData)
        {
            if (mapperData.TargetMember.IsComplex && mapperData.TargetIsDefinitelyPopulated())
            {
                return false;
            }

            return true;
        }

        public bool IsStandalone { get; }

        public bool IsForDerivedType { get; }

        public bool IsForNewElement { get; set; }

        public bool NeedsSubMapping { get; private set; }

        public void SubMappingNeeded()
        {
            if (NeedsSubMapping)
            {
                return;
            }

            NeedsSubMapping = true;
            BubbleMappingNeededToParent();
        }

        private void BubbleMappingNeededToParent()
        {
            if (!_mapperData.IsRoot)
            {
                _mapperData.Parent.Context.SubMappingNeeded();
            }
        }

        public bool UseLocalVariable { get; }

        public bool UseMappingTryCatch
        {
            get
            {
                if (_mapperData.IsRoot)
                {
                    return true;
                }

                if (_mapperData.TargetMemberIsUserStruct())
                {
                    return false;
                }

                return _mapperData.Parent.Context.UseMappingTryCatch;
            }
        }

        public bool UsesMappingDataObjectAsParameter
        {
            get
            {
                return _mapperData.Context._usesMappingDataObjectAsParameter ||
                       _mapperData.ChildMapperDatas.Any(cmd => cmd.Context.UsesMappingDataObjectAsParameter);
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
                return (_isMappingDataObjectNeeded ??
                       (_isMappingDataObjectNeeded =
                           NeedsSubMapping || UsesMappingDataObjectAsParameter ||
                           _mapperData.ChildMapperDatas.Any(cmd => cmd.Context.UsesMappingDataObject))).Value;
            }
        }

        public bool Compile => IsStandalone && !IsForDerivedType;
    }
}