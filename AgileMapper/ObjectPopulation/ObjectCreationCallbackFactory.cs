namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using Api.Configuration;
    using Members;

    internal class ObjectCreationCallbackFactory : UserConfiguredItemBase
    {
        private readonly Type _creationTargetType;
        private readonly CallbackPosition _callbackPosition;
        private readonly ConfiguredLambdaInfo _callbackLambda;

        public ObjectCreationCallbackFactory(
            MappingConfigInfo configInfo,
            Type mappingTargetType,
            Type creationTargetType,
            CallbackPosition callbackPosition,
            ConfiguredLambdaInfo callbackLambda)
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
            var callback = _callbackLambda.GetBody(context);
            var condition = GetCondition(context);

            return new ObjectCreationCallback(_callbackPosition, callback, condition);
        }
    }
}