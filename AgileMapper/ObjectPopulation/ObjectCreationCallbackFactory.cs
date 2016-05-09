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
        private readonly Type _targetType;
        private readonly CallbackPosition _callbackPosition;
        private readonly LambdaExpression _callbackLambda;
        private readonly Func<IMemberMappingContext, Expression[]> _parameterReplacementsFactory;

        public ObjectCreationCallbackFactory(
            MappingConfigInfo configInfo,
            Type targetType,
            CallbackPosition callbackPosition,
            LambdaExpression callbackLambda,
            Func<IMemberMappingContext, Expression[]> parameterReplacementsFactory)
            : base(configInfo, targetType, QualifiedMember.All)
        {
            _targetType = targetType;
            _callbackPosition = callbackPosition;
            _callbackLambda = callbackLambda;
            _parameterReplacementsFactory = parameterReplacementsFactory;
        }

        public override bool AppliesTo(IMemberMappingContext context)
            => _targetType.IsAssignableFrom(context.TargetVariable.Type) && base.AppliesTo(context);

        public ObjectCreationCallback GetCallback(IMemberMappingContext context)
        {
            var parameterReplacements = _parameterReplacementsFactory.Invoke(context);
            var callback = _callbackLambda.ReplaceParameters(parameterReplacements);

            return new ObjectCreationCallback(_callbackPosition, callback);
        }
    }
}