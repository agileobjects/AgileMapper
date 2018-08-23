namespace AgileObjects.AgileMapper.Configuration.Projection
{
    using Members;

    internal class RecursionDepthSettings : UserConfiguredItemBase
    {
        private readonly int _recursionDepth;

        public RecursionDepthSettings(MappingConfigInfo configInfo, int recursionDepth)
            : base(configInfo)
        {
            _recursionDepth = recursionDepth;
        }

        public bool IsBeyondDepth(IBasicMapperData mapperData)
        {
            if (_recursionDepth == 0)
            {
                return true;
            }

            var recursionDepth = -1;

            while (mapperData != null)
            {
                if (mapperData.TargetMember.IsRecursion && 
                    mapperData.RuleSet.RepeatMappingStrategy.AppliesTo(mapperData))
                {
                    ++recursionDepth;
                }

                mapperData = mapperData.Parent;
            }

            return recursionDepth > _recursionDepth;
        }
    }
}
