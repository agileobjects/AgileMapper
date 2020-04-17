namespace AgileObjects.AgileMapper.ObjectPopulation
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Configuration;
    using Configuration.Lambdas;
    using Members;

    internal class MappingCallbackFactory : UserConfiguredItemBase
    {
        private readonly ConfiguredLambdaInfo _callbackLambda;

        public MappingCallbackFactory(
            MappingConfigInfo configInfo,
            ConfiguredLambdaInfo callbackLambda,
            QualifiedMember targetMember)
            : base(configInfo, targetMember)
        {
            _callbackLambda = callbackLambda;
        }

        protected InvocationPosition InvocationPosition => ConfigInfo.InvocationPosition;

        public virtual bool AppliesTo(InvocationPosition invocationPosition, IQualifiedMemberContext context)
            => (InvocationPosition == invocationPosition) && base.AppliesTo(context);

        protected override bool TypesMatch(IQualifiedMemberContext context) => TypesAreCompatible(context);

        public Expression Create(IMemberMapperData mapperData)
        {
            mapperData.Context.UsesMappingDataObjectAsParameter =
               _callbackLambda.UsesMappingDataObjectParameter ||
                ConfigInfo.ConditionUsesMappingDataObjectParameter;

            var condition = GetConditionOrNull(mapperData);
            var callback = _callbackLambda.GetBody(mapperData);

            return (condition != null) ? Expression.IfThen(condition, callback) : callback;
        }
    }
}