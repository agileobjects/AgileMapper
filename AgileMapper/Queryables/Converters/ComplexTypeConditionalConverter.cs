namespace AgileObjects.AgileMapper.Queryables.Converters
{
    using System.Linq.Expressions;
    using Extensions.Internal;
    using ObjectPopulation;
    using ReadableExpressions.Extensions;
    using Settings;

    internal static class ComplexTypeConditionalConverter
    {
        public static bool TryConvert(
            ConditionalExpression conditional,
            IQueryProviderSettings settings,
            ObjectMapperData mapperData,
            out Expression converted)
        {
            if (settings.SupportsNonEntityNullConstants || !conditional.Type.IsComplex())
            {
                converted = null;
                return false;
            }

            converted = new NonNullableMemberBinder(conditional).GuardMemberAccesses();
            return true;
        }

        public class NonNullableMemberBinder : ExpressionVisitor
        {
            private readonly ConditionalExpression _conditional;

            public NonNullableMemberBinder(ConditionalExpression conditional)
            {
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
                    NullConstantExpressionFactory.CreateFor(memberBinding.Expression));

                return memberBinding.Update(bindingValueOrNull);
            }
        }
    }
}