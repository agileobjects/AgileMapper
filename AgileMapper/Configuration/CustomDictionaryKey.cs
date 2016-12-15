namespace AgileObjects.AgileMapper.Configuration
{
    using System.Linq.Expressions;

    internal class CustomDictionaryKey : UserConfiguredItemBase
    {
        public CustomDictionaryKey(
            string key,
            LambdaExpression targetMemberLambda,
            MappingConfigInfo configInfo)
            : base(configInfo, targetMemberLambda)
        {
            Key = key;
        }

        public string Key { get; }
    }
}