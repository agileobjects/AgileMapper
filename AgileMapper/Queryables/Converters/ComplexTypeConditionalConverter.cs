namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ReadableExpressions.Extensions;
    using Settings;

    internal static class ComplexTypeConditionalConverter
    {
        public static bool TryConvert(
            ConditionalExpression conditional,
            IQueryProjectionModifier modifier,
            out Expression converted)
        {
            if (modifier.Settings.SupportsNonEntityNullConstants || !conditional.Type.IsComplex())
            {
                converted = null;
                return false;
            }

            converted = new NonNullableMemberBinder(modifier, conditional).GuardMemberAccesses();
            return true;
        }

        private class NonNullableMemberBinder : ExpressionVisitor
        {
            private readonly IQueryProviderSettings _settings;
            private readonly ConditionalExpression _conditional;

            public NonNullableMemberBinder(IQueryProjectionModifier modifier, ConditionalExpression conditional)
            {
                _settings = modifier.Settings;
                _conditional = conditional;
            }

            public Expression GuardMemberAccesses()
                => VisitAndConvert(_conditional.IfTrue, nameof(GuardMemberAccesses));

            protected override MemberBinding VisitMemberBinding(MemberBinding binding)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    return base.VisitMemberBinding(binding);
                }

                var memberBinding = (MemberAssignment)binding;

                if (memberBinding.Expression.Type.CanBeNull())
                {
                    return base.VisitMemberBinding(binding);
                }

                var bindingValueOrNull = Expression.Condition(
                    _conditional.Test,
                    memberBinding.Expression,
                    _settings.GetDefaultValueFor(memberBinding.Expression));

                return memberBinding.Update(bindingValueOrNull);
            }
        }
    }
}