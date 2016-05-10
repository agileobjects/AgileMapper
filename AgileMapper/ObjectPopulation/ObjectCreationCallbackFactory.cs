namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using DataSources;
    using Extensions;
    using Members;

    internal class ObjectCreationCallbackFactory : UserConfiguredItemBase
    {
        private readonly Type _creationTargetType;
        private readonly CallbackPosition _callbackPosition;
        private readonly LambdaExpression _callbackLambda;

        public ObjectCreationCallbackFactory(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Type creationTargetType,
            CallbackPosition callbackPosition,
            LambdaExpression callbackLambda)
            : base(configInfo, mappingTargetType, QualifiedMember.All)
        {
            _creationTargetType = creationTargetType;
            _callbackPosition = callbackPosition;
            _callbackLambda = callbackLambda;
        }

        public override bool AppliesTo(IMemberMappingContext context)
            => _creationTargetType.IsAssignableFrom(context.InstanceVariable.Type) && base.AppliesTo(context);

        public ObjectCreationCallback GetCallback(IMemberMappingContext context)
        {
            var callback = _callbackLambda.ReplaceParameterWith(context.Parameter);
            var condition = GetCondition(context.Parameter);

            return new ObjectCreationCallback(_callbackPosition, callback, condition);
        }
    }
}