namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using Members;

    internal class ConfiguredObjectFactory : UserConfiguredItemBase
    {
        private readonly Type _objectType;
        private readonly ConfiguredLambdaInfo _objectFactory;

        private ConfiguredObjectFactory(
            MappingConfigInfo configInfo,
            Type objectType,
            LambdaExpression objectFactory)
            : base(configInfo)
        {
            _objectType = objectType;
            _objectFactory = ConfiguredLambdaInfo.For(objectFactory);
        }

        #region Factory Methods

        public static ConfiguredObjectFactory For(
            MapperContext mapperContext,
            Type objectType,
            LambdaExpression factory)
            => new ConfiguredObjectFactory(
                   new MappingConfigInfo(mapperContext).ForAllRuleSets().ForAllSourceTypes().ForAllTargetTypes(),
                   objectType,
                   factory);

        public static ConfiguredObjectFactory For(
            MappingConfigInfo configInfo,
            Type objectType,
            LambdaExpression factory)
            => new ConfiguredObjectFactory(configInfo.ForTargetType(objectType), objectType, factory);

        #endregion

        public override bool AppliesTo(IMappingData data)
            => _objectType.IsAssignableFrom(data.TargetMember.Type) && base.AppliesTo(data);

        public Expression Create(IMemberMappingContext context) => _objectFactory.GetBody(context);
    }
}