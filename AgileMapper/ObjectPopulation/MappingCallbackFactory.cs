namespace AgileObjects.AgileMapper.ObjectPopulation;

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

    protected InvocationPosition InvocationPosition 
        => ConfigInfo.InvocationPosition;

    public virtual bool AppliesTo(
        InvocationPosition invocationPosition, 
        IQualifiedMemberContext context)
    {
        return InvocationPosition == invocationPosition && base.AppliesTo(context);
    }

    protected override bool TypesMatch(IQualifiedMemberContext context) 
        => TypesAreCompatible(context);

    public Expression Create(IMemberMapperData mapperData)
    {
        mapperData.Context.NeedsMappingData =
            _callbackLambda.NeedsMappingData ||
            ConfigInfo.ConditionNeedsMappingData;

        var condition = GetConditionOrNull(mapperData);
        var callback = GetCallbackBody(mapperData);

        return condition != null ? Expression.IfThen(condition, callback) : callback;
    }

    protected virtual Expression GetCallbackBody(IMemberMapperData mapperData) 
        => _callbackLambda.GetBody(mapperData);
}