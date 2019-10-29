namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using Members;
    using NetStandardPolyfills;

    internal class ConfiguredObjectFactory :
        UserConfiguredItemBase,
        IPotentialAutoCreatedItem
#if NET35
        , IComparable<ConfiguredObjectFactory>
#endif
    {
        private readonly ConfiguredLambdaInfo _factoryInfo;

        public ConfiguredObjectFactory(
            MappingConfigInfo configInfo,
            Type objectType,
            ConfiguredLambdaInfo factoryInfo)
            : base(configInfo)
        {
            ObjectType = objectType;
            _factoryInfo = factoryInfo;
        }

        public Type ObjectType { get; }

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

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return ObjectType.IsAssignableTo(mapperData.TargetType) &&
                   base.AppliesTo(mapperData) &&
                  _factoryInfo.Supports(mapperData.RuleSet);
        }

        public Expression Create(IMemberMapperData mapperData) => _factoryInfo.GetBody(mapperData);

        #region IPotentialAutoCreatedItem Members

        public bool WasAutoCreated { get; private set; }

        public IPotentialAutoCreatedItem Clone()
        {
            return new ConfiguredObjectFactory(ConfigInfo, ObjectType, _factoryInfo)
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