namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class ExceptionCallbackFactory : UserConfiguredItemBase
    {
        private readonly Expression _callback;

        public ExceptionCallbackFactory(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Expression callback)
            : base(configInfo, mappingTargetType)
        {
            _callback = callback;
        }

        public ExceptionCallback Create(IMemberMappingContext context) => new ExceptionCallback(_callback);
    }
}