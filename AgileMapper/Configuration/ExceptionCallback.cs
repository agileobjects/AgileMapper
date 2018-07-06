namespace AgileObjects.AgileMapper.Configuration
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

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