namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
    using Configuration;
    using Members;

    internal class ConfiguredObjectFactory : UserConfiguredItemBase
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

        public override bool AppliesTo(IBasicMapperData mapperData)
            => _objectType.IsAssignableFrom(mapperData.TargetType) && base.AppliesTo(mapperData);

        public Expression Create(IMemberMapperData mapperData) => _factoryInfo.GetBody(mapperData);
    }
}