namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using static System.StringComparison;
    using static Constants;

    internal static class MemberExtensions
    {
        public static string GetFullName(this IEnumerable<Member> members)
            => members.Project(m => m.JoiningName).Join(string.Empty);

        public static string GetFriendlySourcePath(this IQualifiedMember sourceMember, IMemberMapperData rootMapperData)
            => GetFriendlyMemberPath(sourceMember, rootMapperData.SourceMember);

        public static string GetFriendlyTargetPath(this IQualifiedMember targetMember, IMemberMapperData rootMapperData)
            => GetFriendlyMemberPath(targetMember, rootMapperData.TargetMember);

        public static string GetFriendlyMemberPath(this IQualifiedMember member, IQualifiedMember rootMember)
        {
            var rootTypeName = rootMember.GetFriendlyTypeName();
            var memberPath = member.GetPath();

            if (memberPath == rootMember.Name)
            {
                return rootTypeName;
            }

            if (memberPath.StartsWith(rootMember.Name, Ordinal))
            {
                return rootTypeName + memberPath.Substring(rootMember.Name.Length);
            }

            var rootMemberNameIndex = memberPath.IndexOf("." + rootMember.Name + ".", Ordinal);

            if (rootMemberNameIndex == -1)
            {
                return rootTypeName + memberPath;
            }

            var rootMemberString = memberPath.Substring(rootMemberNameIndex + rootMember.Name.Length + 2);
            var path = rootTypeName + "." + rootMemberString;

            return path;
        }

        public static bool IsEntityId(this Member member)
            => member.MemberInfo?.HasKeyAttribute() == true;

        public static bool IsUnmappable(this QualifiedMember member, out string reason)
        {
            if (member.MemberChain.Length < 2)
            {
                // Either the root member, QualifiedMember.All or QualifiedMember.None:
                reason = null;
                return false;
            }

            if (IsStructNonSimpleMember(member))
            {
                reason = member.Type.GetFriendlyName() + " member on a struct";
                return true;
            }

            if (member.LeafMember.MemberType == MemberType.SetMethod)
            {
                reason = null;
                return false;
            }

            if (!member.IsReadable)
            {
                reason = "write-only member";
                return true;
            }

            if (!member.IsReadOnly)
            {
                reason = null;
                return false;
            }

            if (member.Type.IsArray)
            {
                reason = "readonly array";
                return true;
            }

            if (member.IsSimple || member.Type.IsValueType())
            {
                reason = "readonly " + ((member.IsComplex) ? "struct" : member.Type.GetFriendlyName());
                return true;
            }

            if (member.IsEnumerable && member.Type.IsClosedTypeOf(typeof(ReadOnlyCollection<>)))
            {
                reason = "readonly " + member.Type.GetFriendlyName();
                return true;
            }

            reason = null;
            return false;
        }

        private static bool IsStructNonSimpleMember(QualifiedMember member)
        {
            if (member.IsSimple || member.Type.IsValueType())
            {
                return false;
            }

            return member.MemberChain[member.MemberChain.Length - 2].Type.IsValueType();
        }

        public static Expression GetAccess(this QualifiedMember member, IMemberMapperData mapperData)
            => member.GetAccess(mapperData.TargetInstance, mapperData);

        public static Expression GetQualifiedAccess(this IQualifiedMember sourceMember, IMemberMapperData mapperData)
            => sourceMember.GetQualifiedAccess(mapperData.SourceObject);

        public static Expression GetQualifiedAccess(this IEnumerable<Member> memberChain, Expression parentInstance)
        {
            // Skip(1) because the 0th member is the mapperData.SourceObject:
            return memberChain.Skip(1).Aggregate(
                parentInstance,
                (accessSoFar, member) => member.GetAccess(accessSoFar));
        }

        [DebuggerStepThrough]
        public static bool IsEnumerableElement(this Member member) => member.MemberType == MemberType.EnumerableElement;

        public static IList<string> ExtendWith(
            this ICollection<string> parentJoinedNames,
            string[] memberMatchingNames,
            MapperContext mapperContext)
        {
            return mapperContext.Naming.ExtendJoinedNames(parentJoinedNames, memberMatchingNames);
        }

        public static bool CouldMatch(this IList<string> memberNames, IList<string> otherMemberNames)
        {
            if (otherMemberNames.HasOne() && (otherMemberNames.First() == RootMemberName) ||
                memberNames.HasOne() && (memberNames.First() == RootMemberName))
            {
                return true;
            }

            return otherMemberNames
                .Any(otherJoinedName => (otherJoinedName == RootMemberName) || memberNames
                    .Any(joinedName => (joinedName == RootMemberName) || otherJoinedName.StartsWithIgnoreCase(joinedName)));
        }

        public static bool Match(this ICollection<string> memberNames, ICollection<string> otherMemberNames)
        {
            if (!memberNames.HasOne())
            {
                return memberNames
                    .Intersect(otherMemberNames, StringComparer.OrdinalIgnoreCase)
                    .Any();
            }

            var memberName = memberNames.First();

            return otherMemberNames.HasOne()
                ? memberName.EqualsIgnoreCase(otherMemberNames.First())
                : otherMemberNames.Any(otherMemberName => otherMemberName.EqualsIgnoreCase(memberName));
        }

        public static TMember GetElementMember<TMember>(this TMember enumerableMember)
            where TMember : IQualifiedMember
            => (TMember)enumerableMember.Append(enumerableMember.Type.GetElementMember());

        public static Member GetElementMember(this Type enumerableType)
            => GlobalContext.Instance.MemberCache.GetSourceMembers(enumerableType).First();

        public static Member[] RelativeTo(this Member[] memberChain, Member[] otherMemberChain)
        {
            var otherMembersLeafMember = otherMemberChain.Last();

            if (memberChain.HasOne() && (memberChain[0] == otherMembersLeafMember))
            {
                return memberChain;
            }

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
            => Expression.Call(instance, targetMember.Name, NoTypeArguments, value);

        #endregion

        public static Expression GetPopulation(this Member targetMember, Expression instance, Expression value)
        {
            var populationFactory = _populationFactoriesByMemberType[targetMember.MemberType];
            var population = populationFactory.Invoke(instance, targetMember, value);

            return population;
        }

#if NET35
        public static QualifiedMember ToSourceMember(this LinqExp.Expression memberAccess, MapperContext mapperContext)
            => memberAccess.ToDlrExpression().ToSourceMember(mapperContext);
#endif
        public static QualifiedMember ToSourceMember(this Expression memberAccess, MapperContext mapperContext)
        {
            return CreateMember(
                memberAccess,
                Member.RootSource,
                GlobalContext.Instance.MemberCache.GetSourceMembers,
                mapperContext);
        }

#if NET35
        public static QualifiedMember ToTargetMember(this LinqExp.LambdaExpression memberAccess, MapperContext mapperContext)
            => memberAccess.ToDlrExpression().ToTargetMember(mapperContext);
#endif
        public static QualifiedMember ToTargetMember(this LambdaExpression memberAccess, MapperContext mapperContext)
        {
            return CreateMember(
                memberAccess.Body,
                Member.RootTarget,
                GlobalContext.Instance.MemberCache.GetTargetMembers,
                mapperContext);
        }

        internal static QualifiedMember CreateMember(
            Expression memberAccessExpression,
            Func<Type, Member> rootMemberFactory,
            Func<Type, IList<Member>> membersFactory,
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

            AdjustMemberAccessesIfRootedInMappingData(memberAccesses, ref expression);

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

        private static void AdjustMemberAccessesIfRootedInMappingData(IList<Expression> memberAccesses, ref Expression expression)
        {
            if (!expression.Type.IsClosedTypeOf(typeof(IMappingData<,>)))
            {
                return;
            }

            var mappingDataRoot = memberAccesses[0];
            expression = Parameters.Create(mappingDataRoot.Type);

            memberAccesses.RemoveAt(0);

            for (var i = 0; i < memberAccesses.Count; i++)
            {
                memberAccesses[i] = memberAccesses[i].Replace(mappingDataRoot, expression);
            }
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
#if NET35
            var methodInfoValue = methodCall.Arguments.Last();
            var instance = methodCall.Arguments[1];
#else
            var methodInfoValue = methodCall.Object;
            var instance = methodCall.Arguments.Last();
#endif
            // ReSharper disable once PossibleNullReferenceException
            var methodInfo = (MethodInfo)((ConstantExpression)methodInfoValue).Value;

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
