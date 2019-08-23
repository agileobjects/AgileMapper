namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
#if NET35
    using System;
#endif

    internal abstract class ConfiguredIgnoredSourceMemberBase :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredIgnoredSourceMemberBase>
#endif
    {
        protected ConfiguredIgnoredSourceMemberBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        public abstract string GetConflictMessage(ConfiguredIgnoredSourceMemberBase conflictingIgnoredSourceMember);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; protected set; }

        public abstract IPotentialAutoCreatedItem Clone();

        public abstract bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem);

        #endregion

#if NET35
        int IComparable<ConfiguredIgnoredSourceMemberBase>.CompareTo(ConfiguredIgnoredSourceMemberBase other)
            => DoComparisonTo(other);
#endif
    }
}