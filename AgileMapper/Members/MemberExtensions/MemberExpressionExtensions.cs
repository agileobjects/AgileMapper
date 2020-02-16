namespace AgileObjects.AgileMapper.Members.MemberExtensions
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MemberExpressionExtensions
    {
        public static bool IsMappingDataObjectCall(
            this MethodCallExpression methodCall,
            Expression rootMappingDataObject)
        {
            return (methodCall.Method.DeclaringType == typeof(IMappingData)) ||
                   (methodCall.Object == rootMappingDataObject);
        }

        public static TryExpression WrapInTryCatch(this Expression mapping, IMemberMapperData mapperData)
        {
            var configuredCallback = mapperData.MapperContext.UserConfigurations.GetExceptionCallbackOrNull(mapperData);
            var exceptionVariable = Parameters.Create<Exception>("ex");

            if (configuredCallback == null)
            {
                var catchBody = Expression.Throw(
                    MappingException.GetFactoryMethodCall(mapperData, exceptionVariable),
                    mapping.Type);

                return CreateTryCatch(mapping, exceptionVariable, catchBody);
            }

            var configuredCatchBody = configuredCallback
                .ToCatchBody(exceptionVariable, mapping.Type, mapperData);

            return CreateTryCatch(mapping, exceptionVariable, configuredCatchBody);
        }

        private static TryExpression CreateTryCatch(
            Expression mappingBlock,
            ParameterExpression exceptionVariable,
            Expression catchBody)
        {
            var catchBlock = Expression.Catch(exceptionVariable, catchBody);

            return Expression.TryCatch(mappingBlock, catchBlock);
        }
    }
}