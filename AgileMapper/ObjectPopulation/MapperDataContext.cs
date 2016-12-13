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
                childMapperData.Parent.Context.IsForDerivedType)
        {
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
        }

        public bool IsStandalone { get; }

        public bool IsForDerivedType { get; }

        public bool IsForNewElement { get; set; }

        public bool NeedsChildMapping { get; private set; }

        public void ChildMappingNeeded()
        {
            if (NeedsChildMapping)
            {
                return;
            }

            NeedsChildMapping = true;
            BubbleMappingNeededToParent();
        }

        public bool NeedsElementMapping { get; private set; }

        public void ElementMappingNeeded()
        {
            if (NeedsElementMapping)
            {
                return;
            }

            NeedsElementMapping = true;
            BubbleMappingNeededToParent();
        }

        private void BubbleMappingNeededToParent()
        {
            if (_mapperData.IsRoot)
            {
                return;
            }

            if (_mapperData.TargetMemberIsEnumerableElement())
            {
                _mapperData.Parent.Context.ElementMappingNeeded();
                return;
            }

            _mapperData.Parent.Context.ChildMappingNeeded();
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
                            NeedsChildMapping || NeedsElementMapping || UsesMappingDataObjectAsParameter ||
                            _mapperData.ChildMapperDatas.Any(cmd => cmd.Context.UsesMappingDataObject))).Value;
            }
        }

        public bool Compile => IsStandalone && !IsForDerivedType;
    }
}