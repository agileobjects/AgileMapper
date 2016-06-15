namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
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

        public static ConfiguredObjectFactory For(
            MapperContext mapperContext,
            Type objectType,
            LambdaExpression factory)
            => For(mapperContext, objectType, ConfiguredLambdaInfo.For(factory));

        public static ConfiguredObjectFactory For(
            MapperContext mapperContext,
            Type objectType,
            ConfiguredLambdaInfo factoryInfo)
            => new ConfiguredObjectFactory(
                   MappingConfigInfo.AllRuleSetsSourceTypesAndTargetTypes(mapperContext),
                   objectType,
                   factoryInfo);

        public static ConfiguredObjectFactory For(MappingConfigInfo configInfo, Type objectType, LambdaExpression factory)
            => For(configInfo, objectType, ConfiguredLambdaInfo.For(factory));

        public static ConfiguredObjectFactory For(
            MappingConfigInfo configInfo,
            Type objectType,
            ConfiguredLambdaInfo factoryInfo)
            => new ConfiguredObjectFactory(configInfo.ForTargetType(objectType), objectType, factoryInfo);

        #endregion

        public override bool AppliesTo(IMappingData data)
            => _objectType.IsAssignableFrom(data.TargetMember.Type) && base.AppliesTo(data);

        public Expression Create(IMemberMappingContext context) => _factoryInfo.GetBody(context);
    }
}