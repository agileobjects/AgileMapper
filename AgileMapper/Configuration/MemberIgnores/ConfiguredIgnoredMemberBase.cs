namespace AgileObjects.AgileMapper.Configuration.MemberIgnores
{
#if NET35
    using System;
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using DataSources.Factories;
    using Members;

    internal abstract class ConfiguredIgnoredMemberBase :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem,
        IReverseConflictable
#if NET35
        , IComparable<ConfiguredIgnoredMemberBase>
#endif
    {
        protected ConfiguredIgnoredMemberBase(MappingConfigInfo configInfo)
            : base(configInfo)
        {
        }

        protected ConfiguredIgnoredMemberBase(MappingConfigInfo configInfo, LambdaExpression targetMemberLambda)
            : base(configInfo, targetMemberLambda)
        {
        }

        protected ConfiguredIgnoredMemberBase(MappingConfigInfo configInfo, QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
        }

        public string GetConflictMessage(UserConfiguredItemBase conflictingConfiguredItem)
        {
            if (conflictingConfiguredItem is ConfiguredDataSourceFactory conflictingDataSource)
            {
                return GetConflictMessage(conflictingDataSource);
            }

            if (conflictingConfiguredItem is ConfiguredIgnoredMemberBase conflictingMemberIgnore)
            {
                return GetConflictMessage(conflictingMemberIgnore);
            }

            return $"Member {TargetMember.GetPath()} has been ignored";
        }

        public abstract string GetConflictMessage(ConfiguredIgnoredMemberBase conflictingIgnoredMember);

        public abstract string GetConflictMessage(ConfiguredDataSourceFactory conflictingDataSource);

        public abstract string GetIgnoreMessage(IQualifiedMember targetMember);

        protected override bool HasReverseConflict(UserConfiguredItemBase otherItem) => false;

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; protected set; }

        public abstract IPotentialAutoCreatedItem Clone();

        public abstract bool IsReplacementFor(IPotentialAutoCreatedItem autoCreatedItem);

        #endregion

#if NET35
        int IComparable<ConfiguredIgnoredMemberBase>.CompareTo(ConfiguredIgnoredMemberBase other)
            => DoComparisonTo(other);
#endif
    }
}