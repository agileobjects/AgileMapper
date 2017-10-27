namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Configuration;
    using Members;
    using ReadableExpressions.Extensions;

    internal class ConfiguredObjectFactory : UserConfiguredItemBase, IPotentialClone
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
            => mapperData.TargetType.IsAssignableFrom(_objectType) && base.AppliesTo(mapperData);

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
    }
}