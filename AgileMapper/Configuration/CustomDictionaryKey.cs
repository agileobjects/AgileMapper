namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;

    internal class CustomDictionaryKey : UserConfiguredItemBase
    {
        public CustomDictionaryKey(
            Expression keyValue,
            LambdaExpression targetMemberLambda,
            MappingConfigInfo configInfo)
            : base(configInfo, targetMemberLambda)
        {
            KeyValue = keyValue;
        }

        public Expression KeyValue { get; }
    }
}