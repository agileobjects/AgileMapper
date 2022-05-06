namespace AgileObjects.AgileMapper.ObjectPopulation;

using System;
#if NET35
using Microsoft.Scripting.Ast;
#else
using System.Linq.Expressions;
#endif
using Configuration;
using Configuration.Lambdas;
using Extensions.Internal;
using Members;
using NetStandardPolyfills;
using static InvocationPosition;

internal class ObjectCreationCallbackFactory : MappingCallbackFactory
{
    private readonly Type _creationTargetType;

    public ObjectCreationCallbackFactory(
        MappingConfigInfo configInfo,
        Type creationTargetType,
        ConfiguredLambdaInfo callbackLambda)
        : base(configInfo, callbackLambda, QualifiedMember.All)
    {
        _creationTargetType = creationTargetType;
    }

    public override bool AppliesTo(
        InvocationPosition invocationPosition,
        IQualifiedMemberContext context)
    {
        return context.TargetMember.Type.IsAssignableTo(_creationTargetType) &&
               base.AppliesTo(invocationPosition, context);
    }

    protected override bool TypesMatch(IQualifiedMemberContext context)
        => SourceAndTargetTypesMatch(context);

    public override Expression GetConditionOrNull(IMemberMapperData mapperData)
    {
        var condition = base.GetConditionOrNull(mapperData);

        if (InvocationPosition == Before || mapperData.TargetMemberIsUserStruct())
        {
            return condition.RemoveSetTargetCall();
        }

        var newObjectHasBeenCreated = mapperData.CreatedObject.GetIsNotDefaultComparison();

        if (condition == null)
        {
            return newObjectHasBeenCreated;
        }

        return Expression.AndAlso(newObjectHasBeenCreated, condition);
    }

    protected override Expression GetCallbackBody(IMemberMapperData mapperData)
    {
        var callback = base.GetCallbackBody(mapperData);

        if (InvocationPosition == After)
        {
            return callback;
        }

        var createdObject = mapperData.CreatedObject;
        var nullCreatedObject = createdObject.Type.ToDefaultExpression();

        return callback
            .RemoveSetTargetCall()
            .Replace(createdObject, nullCreatedObject);
    }
}