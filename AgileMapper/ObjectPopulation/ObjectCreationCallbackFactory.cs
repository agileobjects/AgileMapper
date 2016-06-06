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
            Type creationTargetType,
            CallbackPosition callbackPosition,
            ConfiguredLambdaInfo callbackLambda)
            : base(configInfo)
        {
            _creationTargetType = creationTargetType;
            _callbackPosition = callbackPosition;
            _callbackLambda = callbackLambda;
        }

        public override bool AppliesTo(IMappingData data)
            => _creationTargetType.IsAssignableFrom(data.TargetMember.Type) && base.AppliesTo(data);

        public ObjectCreationCallback Create(IMemberMappingContext context)
        {
            var callback = _callbackLambda.GetBody(context);
            var condition = GetCondition(context);

            return new ObjectCreationCallback(_callbackPosition, callback, condition);
        }
    }
}