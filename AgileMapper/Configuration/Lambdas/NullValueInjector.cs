namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class NullValueInjector : IValueInjector
    {
        private readonly Expression _lambdaBody;

        public NullValueInjector(LambdaExpression lambda)
        {
            _lambdaBody = lambda.Body;
        }

        public bool HasMappingContextParameter => false;

        public Expression Inject(Type[] contextTypes, IMemberMapperData mapperData) => _lambdaBody;
    }
}