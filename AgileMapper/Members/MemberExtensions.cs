namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Extensions;

    internal static class MemberExtensions
    {
        #region AccessFactoriesByMemberType

        private static readonly Dictionary<MemberType, Func<Expression, Member, Expression>> _accessFactoriesByMemberType =
            new Dictionary<MemberType, Func<Expression, Member, Expression>>
            {
                { MemberType.Field, (instance, member) => Expression.Field(instance, member.Name) },
                { MemberType.Property, (instance, member) => Expression.Property(instance, member.Name) },
                { MemberType.GetMethod, (instance, member) => Expression.Call(instance, member.Name, Constants.NoTypeArguments) }
            };

        #endregion

        public static Expression GetAccess(this Member sourceMember, Expression instance)
        {
            var accessFactory = _accessFactoriesByMemberType[sourceMember.MemberType];
            var access = accessFactory.Invoke(instance, sourceMember);

            return access;
        }

        #region PopulationFactoriesByMemberType

        private delegate Expression PopulationFactory(Expression instance, Member member, Expression value);

        private static readonly Dictionary<MemberType, PopulationFactory> _populationFactoriesByMemberType =
            new Dictionary<MemberType, PopulationFactory>
            {
                { MemberType.Field, AssignMember },
                { MemberType.Property, AssignMember },
                { MemberType.SetMethod, CallSetMethod }
            };


        private static Expression AssignMember(Expression instance, Member targetMember, Expression value)
        {
            return Expression.Assign(targetMember.GetAccess(instance), value);
        }

        private static Expression CallSetMethod(Expression instance, Member targetMember, Expression value)
        {
            return Expression.Call(instance, targetMember.Name, Constants.NoTypeArguments, value);
        }

        #endregion

        public static Expression GetPopulation(this Member targetMember, Expression instance, Expression value)
        {
            var populationFactory = _populationFactoriesByMemberType[targetMember.MemberType];
            var population = populationFactory.Invoke(instance, targetMember, value);

            return population;
        }

        public static QualifiedMember ToTargetMember(this Expression memberAccessExpression, MemberFinder memberFinder)
        {
            var expression = memberAccessExpression;
            var memberAccesses = new List<Expression>();

            while (expression.NodeType != ExpressionType.Parameter)
            {
                var memberExpression = expression.GetMemberAccess();
                memberAccesses.Insert(0, memberExpression);
                expression = memberExpression.GetParentOrNull();
            }

            var rootMember = Member.RootTarget(expression.Type);
            var parentMember = rootMember;

            var memberChain = memberAccesses
                .Select(memberAccess =>
                {
                    var memberName = memberAccess.GetMemberName();
                    var members = memberFinder.GetTargetMembers(parentMember.Type);
                    var member = members.FirstOrDefault(m => m.Name == memberName);

                    parentMember = member;

                    return member;
                })
                .ToList();

            memberChain.Insert(0, rootMember);

            return QualifiedMember.From(memberChain.ToArray());
        }
    }
}
