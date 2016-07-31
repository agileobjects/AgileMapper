namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;

    internal class ExceptionCallback : UserConfiguredItemBase
    {
        public ExceptionCallback(MappingConfigInfo configInfo, Expression callback)
            : base(configInfo)
        {
            Callback = callback;
        }

        public Expression Callback { get; }
    }
}