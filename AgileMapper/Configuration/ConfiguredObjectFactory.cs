namespace AgileObjects.AgileMapper.Configuration
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;

    internal class ConfiguredObjectFactory :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredObjectFactory>
#endif
    {
        private readonly ConfiguredLambdaInfo _factoryInfo;

        public ConfiguredObjectFactory(MappingConfigInfo configInfo, ConfiguredLambdaInfo factoryInfo)
            : base(configInfo)
        {
            _factoryInfo = factoryInfo;
        }

        public Type ObjectType => _factoryInfo.ReturnType;

        public bool UsesMappingDataObjectParameter => _factoryInfo.UsesMappingDataObjectParameter;

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            return !WasAutoCreated ||
                   _factoryInfo.IsSameAs(((ConfiguredObjectFactory)otherConfiguredItem)._factoryInfo);
        }

        protected override bool HasOverlappingTypes(UserConfiguredItemBase otherConfiguredItem)
        {
            return base.HasOverlappingTypes(otherConfiguredItem) &&
                (((ConfiguredObjectFactory)otherConfiguredItem).ObjectType == ObjectType);
        }

        public override bool AppliesTo(IQualifiedMemberContext context)
        {
            return ObjectType.IsAssignableTo(context.TargetType) &&
                   base.AppliesTo(context) &&
                  _factoryInfo.Supports(context.RuleSet);
        }

        public Expression Create(IMemberMapperData mapperData) 
            => _factoryInfo.GetBody(mapperData, CallbackPosition.Before, QualifiedMember.All);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; private set; }

        public IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredObjectFactory(ConfigInfo, _factoryInfo)
            {
                WasAutoCreated = true
            };
        }

        public bool IsReplacementFor(IPotentialAutoCreatedItem clonedObjectFactory)
            => ConflictsWith((ConfiguredObjectFactory)clonedObjectFactory);

        #endregion

#if NET35
        int IComparable<ConfiguredObjectFactory>.CompareTo(ConfiguredObjectFactory other)
            => DoComparisonTo(other);
#endif
    }
}