namespace AgileObjects.AgileMapper.Api.Configuration
{
    using System.Linq.Expressions;
    using Members;

    internal class ExceptionCallbackFactory : UserConfiguredItemBase
    {
        private readonly Expression _callback;

        public ExceptionCallbackFactory(MappingConfigInfo configInfo, Expression callback)
            : base(configInfo)
        {
            _callback = callback;
        }

        public ExceptionCallback Create(IMemberMappingContext context) => new ExceptionCallback(_callback);
    }
}