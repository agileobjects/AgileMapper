namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
    using Members;

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

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (base.ConflictsWith(otherConfiguredItem))
            {
                return true;
            }

            if (otherConfiguredItem is ConfiguredDataSourceFactory configuredDataSource)
            {
                var configuredSourceMember = configuredDataSource.ToSourceMemberOrNull();

                if (configuredSourceMember != null)
                {
                    return ConflictsWith(configuredSourceMember);
                }
            }

            return false;
        }

        protected abstract bool ConflictsWith(QualifiedMember sourceMember);

        public abstract string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource);

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