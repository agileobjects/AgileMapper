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

        public bool IsBeyondDepth(IQualifiedMemberContext context)
        {
            if (_recursionDepth == 0)
            {
                return true;
            }

            var recursionDepth = -1;

            while (context != null)
            {
                if (context.TargetMember.IsRecursion && 
                    context.RuleSet.RepeatMappingStrategy.AppliesTo(context))
                {
                    ++recursionDepth;
                }

                context = context.Parent;
            }

            return recursionDepth > _recursionDepth;
        }
    }
}
