namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Linq;
    using Extensions;
    using Members;

    internal class MapperDataContext
    {
        private readonly ObjectMapperData _mapperData;
        private bool _usesMappingDataObjectAsParameter;
        private bool? _isMappingDataObjectNeeded;

        public MapperDataContext(IMemberMapperData childMapperData)
            : this(
                childMapperData.Parent,
                IsForStandaloneMapping(childMapperData),
                childMapperData.Parent.Context.IsForDerivedType,
                childMapperData)
        {
        }

        private static bool IsForStandaloneMapping(IBasicMapperData mapperData)
            => mapperData.SourceType.RuntimeTypeNeeded() || mapperData.TargetType.RuntimeTypeNeeded();

        public MapperDataContext(ObjectMapperData mapperData, bool isStandalone, bool isForDerivedType)
            : this(mapperData, isStandalone, isForDerivedType, mapperData)
        {
        }

        private MapperDataContext(
            ObjectMapperData mapperData,
            bool isStandalone,
            bool isForDerivedType,
            IBasicMapperData basicMapperData)
        {
            _mapperData = mapperData;
            IsStandalone = isStandalone;
            IsForDerivedType = isForDerivedType;
            UseLocalVariable = isForDerivedType || ShouldUseLocalVariable(basicMapperData);
        }

        private static bool ShouldUseLocalVariable(IBasicMapperData mapperData)
        {
            if (mapperData.TargetMember.IsSimple)
            {
                return false;
            }

            if (mapperData.TargetMember.IsComplex &&
               (mapperData.TargetMember.IsReadOnly || mapperData.TargetIsDefinitelyPopulated()))
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

        public bool UseMappingTryCatch => _mapperData.IsRoot || !IsPartOfUserStructMapping;

        public bool IsPartOfUserStructMapping
        {
            get
            {
                var mapperData = _mapperData;

                while (mapperData != null)
                {
                    if (mapperData.TargetMemberIsUserStruct())
                    {
                        return true;
                    }

                    mapperData = mapperData.Parent;
                }

                return false;
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