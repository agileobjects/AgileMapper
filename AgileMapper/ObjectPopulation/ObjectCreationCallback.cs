namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Linq.Expressions;
    using Api.Configuration;
    using DataSources;
    using Extensions;
    using Members;

    internal class ObjectCreationCallback : UserConfiguredItemBase
    {
        private readonly Type _targetType;
        private readonly LambdaExpression _callback;
        private readonly Func<IMemberMappingContext, Expression[]> _parameterReplacementsFactory;

        public ObjectCreationCallback(
            MappingConfigInfo configInfo,
            Type targetType,
            LambdaExpression callback,
            Func<IMemberMappingContext, Expression[]> parameterReplacementsFactory)
            : base(configInfo, targetType, QualifiedMember.All)
        {
            _targetType = targetType;
            _callback = callback;
            _parameterReplacementsFactory = parameterReplacementsFactory;
        }

        public override bool AppliesTo(IMemberMappingContext context)
            => _targetType.IsAssignableFrom(context.TargetVariable.Type) && base.AppliesTo(context);

        public Expression GetCallback(IMemberMappingContext context)
        {
            var parameterReplacements = _parameterReplacementsFactory.Invoke(context);

            return _callback.ReplaceParameters(parameterReplacements);
        }
    }
}