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

        public ObjectCreationCallback(
            MappingConfigInfo configInfo,
            Type targetType,
            LambdaExpression callback)
            : base(configInfo, targetType, QualifiedMember.All)
        {
            _targetType = targetType;
            _callback = callback;
        }

        public override bool AppliesTo(IMemberMappingContext context)
            => _targetType.IsAssignableFrom(context.TargetVariable.Type) && base.AppliesTo(context);

        public Expression GetCallback(IMemberMappingContext context)
            => _callback.ReplaceParameter(context.TargetVariable);
    }
}