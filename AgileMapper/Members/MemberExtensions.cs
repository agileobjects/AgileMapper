namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Extensions;
    using ReadableExpressions.Extensions;

    internal static class MemberExtensions
    {
        public static string GetFullName(this IEnumerable<Member> members)
            => string.Join(string.Empty, members.Select(m => m.JoiningName));

        public static string GetFriendlySourcePath(this IQualifiedMember sourceMember, IMemberMapperData rootMapperData)
            => GetMemberPath(sourceMember, rootMapperData.SourceMember);

        public static string GetFriendlyTargetPath(this IQualifiedMember targetMember, IMemberMapperData rootMapperData)
            => GetMemberPath(targetMember, rootMapperData.TargetMember);

        private static string GetMemberPath(IQualifiedMember member, IQualifiedMember rootMember)
        {
            var rootTypeName = rootMember.Type.GetFriendlyName();
            var memberPath = member.GetPath();

            if (memberPath == rootMember.Name)
            {
                return rootTypeName;
            }

            if (memberPath.StartsWith(rootMember.Name, StringComparison.Ordinal))
            {
                return rootTypeName + memberPath.Substring(rootMember.Name.Length);
            }

            var rootMemberNameIndex = memberPath.IndexOf("." + rootMember.Name + ".", StringComparison.Ordinal);

            if (rootMemberNameIndex == -1)
            {
                return rootTypeName + memberPath;
            }

            var rootMemberString = memberPath.Substring(rootMemberNameIndex + rootMember.Name.Length + 2);
            var path = rootTypeName + "." + rootMemberString;

            return path;
        }

        public static Expression GetQualifiedAccess(this IEnumerable<Member> memberChain, Expression instance)
        {
            // Skip(1) because the 0th member is the instance:
            return memberChain.Skip(1).Aggregate(instance, (accessSoFar, member) => member.GetAccess(accessSoFar));
        }

        public static bool IsEnumerableElement(this Member member) => member.MemberType == MemberType.EnumerableElement;

        public static ICollection<string> ExtendWith(
            this ICollection<string> parentJoinedNames,
            string[] memberMatchingNames,
            MapperContext mapperContext)
        {
            return mapperContext.NamingSettings.ExtendJoinedNames(parentJoinedNames, memberMatchingNames);
        }

        public static bool CouldMatch(this IEnumerable<string> memberNames, IEnumerable<string> otherMemberNames)
        {
            return otherMemberNames
                .Any(otherJoinedName => memberNames
                    .Any(joinedName => otherJoinedName.StartsWith(joinedName, StringComparison.OrdinalIgnoreCase)));
        }

        public static bool Match(this IEnumerable<string> memberNames, IEnumerable<string> otherMemberNames)
        {
            return memberNames
                .Intersect(otherMemberNames, StringComparer.OrdinalIgnoreCase)
                .Any();
        }

        public static IQualifiedMember GetElementMember(this IQualifiedMember enumerableMember)
            => enumerableMember.Append(enumerableMember.Type.GetElementMember());

        public static Member GetElementMember(this Type enumerableType)
            => GlobalContext.Instance.MemberFinder.GetSourceMembers(enumerableType).First();

        public static Member[] RelativeTo(this Member[] memberChain, Member[] otherMemberChain)
        {
            var otherMembersLeafMember = otherMemberChain.Last();
            var startIndex = memberChain.Length - 1;

            if ((memberChain.Length > 2) &&
                memberChain[startIndex] == memberChain[startIndex - 1])
            {
                // The member chain ends in a 1-to-1, immediately recursive
                // relationship; skip the last element:
                --startIndex;
            }

            while (startIndex >= 0)
            {
                var member = memberChain[startIndex];

                if (member == otherMembersLeafMember)
                {
                    break;
                }

                --startIndex;
            }

            var relativeMemberChain = new Member[memberChain.Length - startIndex];

            for (var i = 0; i < relativeMemberChain.Length; i++)
            {
                relativeMemberChain[i] = memberChain[i + startIndex];
            }

            return relativeMemberChain;
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
            => targetMember.GetAccess(instance).AssignTo(value);

        private static Expression CallSetMethod(Expression instance, Member targetMember, Expression value)
            => Expression.Call(instance, targetMember.Name, Constants.NoTypeArguments, value);

        #endregion

        public static Expression GetPopulation(this Member targetMember, Expression instance, Expression value)
        {
            var populationFactory = _populationFactoriesByMemberType[targetMember.MemberType];
            var population = populationFactory.Invoke(instance, targetMember, value);

            return population;
        }

        public static QualifiedMember ToTargetMember(this Expression memberAccess, MapperContext mapperContext)
        {
            return CreateMember(
                memberAccess,
                Member.RootTarget,
                GlobalContext.Instance.MemberFinder.GetTargetMembers,
                mapperContext);
        }

        internal static QualifiedMember CreateMember(
            Expression memberAccessExpression,
            Func<Type, Member> rootMemberFactory,
            Func<Type, IEnumerable<Member>> membersFactory,
            MapperContext mapperContext)
        {
            var expression = memberAccessExpression;
            var memberAccesses = new List<Expression>();

            while (expression.NodeType != ExpressionType.Parameter)
            {
                var memberExpression = GetMemberAccess(expression);
                memberAccesses.Insert(0, memberExpression);
                expression = memberExpression.GetParentOrNull();
            }

            var rootMember = rootMemberFactory.Invoke(expression.Type);
            var parentMember = rootMember;

            var memberChain = new Member[memberAccesses.Count + 1];
            memberChain[0] = rootMember;

            for (var i = 0; i < memberAccesses.Count; i++)
            {
                var memberAccess = memberAccesses[i];
                var memberName = GetMemberName(memberAccess);
                var members = membersFactory.Invoke(parentMember.Type);
                var member = members.FirstOrDefault(m => m.Name == memberName);

                if (member == null)
                {
                    return null;
                }

                memberChain[i + 1] = member;
                parentMember = member;
            }

            return QualifiedMember.From(memberChain, mapperContext);
        }

        private static Expression GetMemberAccess(Expression expression)
        {
            while (true)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        continue;

                    case ExpressionType.Call:
                        return GetMethodCallMemberAccess((MethodCallExpression)expression);

                    case ExpressionType.Lambda:
                        expression = ((LambdaExpression)expression).Body;
                        continue;

                    case ExpressionType.MemberAccess:
                        return expression;
                }

                throw new NotSupportedException("Unable to get member access from " + expression.NodeType + " Expression");
            }
        }

        private static Expression GetMethodCallMemberAccess(MethodCallExpression methodCall)
        {
            if ((methodCall.Type != typeof(Delegate)) || (methodCall.Method.Name != "CreateDelegate"))
            {
                return methodCall;
            }

            // ReSharper disable once PossibleNullReferenceException
            var methodInfo = (MethodInfo)((ConstantExpression)methodCall.Object).Value;
            var instance = methodCall.Arguments.Last();
            var valueParameter = Parameters.Create(methodInfo.GetParameters().First().ParameterType, "value");

            return Expression.Call(instance, methodInfo, valueParameter);
        }

        public static string GetMemberName(this Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Call:
                    return ((MethodCallExpression)expression).Method.Name;

                case ExpressionType.MemberAccess:
                    return ((MemberExpression)expression).Member.Name;
            }

            throw new NotSupportedException("Unable to get member name of " + expression.NodeType + " Expression");
        }
    }
}
