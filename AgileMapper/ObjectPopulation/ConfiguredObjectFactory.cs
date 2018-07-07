namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Configuration;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class ConfiguredObjectFactory :
        UserConfiguredItemBase,
        IPotentialClone
#if NET35
        , IComparable<ConfiguredObjectFactory>
#endif
    {
        private readonly Type _objectType;
        private readonly ConfiguredLambdaInfo _factoryInfo;

        private ConfiguredObjectFactory(
            MappingConfigInfo configInfo,
            Type objectType,
            ConfiguredLambdaInfo factoryInfo)
            : base(configInfo)
        {
            _objectType = objectType;
            _factoryInfo = factoryInfo;
        }

        #region Factory Methods

        public static ConfiguredObjectFactory For(MappingConfigInfo configInfo, Type objectType, LambdaExpression factory)
            => For(configInfo, objectType, ConfiguredLambdaInfo.For(factory));

        public static ConfiguredObjectFactory For(
            MappingConfigInfo configInfo,
            Type objectType,
            ConfiguredLambdaInfo factoryInfo)
            => new ConfiguredObjectFactory(configInfo, objectType, factoryInfo);

        #endregion

        public string ObjectTypeName => _objectType.GetFriendlyName();

        public bool UsesMappingDataObjectParameter => _factoryInfo.UsesMappingDataObjectParameter;

        public override bool ConflictsWith(UserConfiguredItemBase otherConfiguredItem)
        {
            if (!base.ConflictsWith(otherConfiguredItem))
            {
                return false;
            }

            return !IsClone ||
                   _factoryInfo.IsSameAs(((ConfiguredObjectFactory)otherConfiguredItem)._factoryInfo);
        }

        protected override bool HasOverlappingTypes(UserConfiguredItemBase otherConfiguredItem)
        {
            return base.HasOverlappingTypes(otherConfiguredItem) &&
                (((ConfiguredObjectFactory)otherConfiguredItem)._objectType == _objectType);
        }

        public override bool AppliesTo(IBasicMapperData mapperData)
        {
            return _objectType.IsAssignableTo(mapperData.TargetType) &&
                   base.AppliesTo(mapperData) &&
                  _factoryInfo.Supports(mapperData.RuleSet);
        }

        public Expression Create(IMemberMapperData mapperData) => _factoryInfo.GetBody(mapperData);

        #region IPotentialClone Members

        public bool IsClone { get; private set; }

        public IPotentialClone Clone()
        {
            return new ConfiguredObjectFactory(ConfigInfo, _objectType, _factoryInfo)
            {
                IsClone = true
            };
        }

        public bool IsReplacementFor(IPotentialClone clonedObjectFactory)
            => ConflictsWith((ConfiguredObjectFactory)clonedObjectFactory);

        #endregion

#if NET35
        int IComparable<ConfiguredObjectFactory>.CompareTo(ConfiguredObjectFactory other)
            => DoComparisonTo(other);
#endif
    }
}