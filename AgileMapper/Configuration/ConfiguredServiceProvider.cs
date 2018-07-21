namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Extensions.Internal;

    internal class ConfiguredServiceProvider : UserConfiguredItemBase
    {
        private ConstantExpression _serviceFactory;

        public ConfiguredServiceProvider(MappingConfigInfo configInfo, Func<Type, object> serviceFactory)
            : base(configInfo)
        {
            _serviceFactory = serviceFactory.ToConstantExpression();
        }
    }
}