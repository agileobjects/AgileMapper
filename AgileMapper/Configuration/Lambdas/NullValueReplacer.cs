namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;

    internal class NullValueReplacer : IValueReplacer
    {
        private readonly Expression _lambdaBody;

        public NullValueReplacer(LambdaExpression lambda)
        {
            _lambdaBody = lambda.Body;
        }

        public bool NeedsMappingData => false;

        public Expression Replace(Type[] contextTypes, IMemberMapperData mapperData) => _lambdaBody;
    }
}