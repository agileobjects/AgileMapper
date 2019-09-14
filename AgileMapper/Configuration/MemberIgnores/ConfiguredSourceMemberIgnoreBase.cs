namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
#if NET35
    using System;
#endif

    internal abstract class ConfiguredSourceMemberIgnoreBase :
        UserConfiguredItemBase,
        IMemberIgnoreBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredSourceMemberIgnoreBase>
#endif
    {
        protected ConfiguredSourceMemberIgnoreBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        public abstract string GetConflictMessage(ConfiguredSourceMemberIgnoreBase conflictingSourceMemberIgnore);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; protected set; }

        public abstract IPotentialAutoCreatedItem Clone();

        public abstract bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem);

        #endregion

#if NET35
        int IComparable<ConfiguredSourceMemberIgnoreBase>.CompareTo(ConfiguredSourceMemberIgnoreBase other)
            => DoComparisonTo(other);
#endif
    }
}