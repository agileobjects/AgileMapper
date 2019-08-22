namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
    using LinqExp = System.Linq.Expressions;
#else
    using System.Linq.Expressions;
#endif
    using System.Reflection;
    using Configuration;
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using ReadableExpressions;
    using ReadableExpressions.Extensions;
    using static System.StringComparison;
    using static Constants;
    using static Member;

    internal static class MemberExtensions
    {
        public static string GetFullName(this IEnumerable<Member> members)
            => members.Project(m => m.JoiningName).Join(string.Empty);

        public static string GetFriendlySourcePath(this IQualifiedMember sourceMember, IMemberMapperData rootMapperData)
            => GetFriendlyMemberPath(sourceMember, rootMapperData.SourceMember);

        public static string GetFriendlySourcePath(this IQualifiedMember targetMember, MappingConfigInfo configInfo)
            => GetFriendlyMemberPath(targetMember, configInfo.SourceType.GetFriendlyName(), RootSourceMemberName);

        public static string GetFriendlyTargetPath(this IQualifiedMember targetMember, IMemberMapperData rootMapperData)
            => GetFriendlyMemberPath(targetMember, rootMapperData.TargetMember);

        public static string GetFriendlyTargetPath(this IQualifiedMember targetMember, MappingConfigInfo configInfo)
            => GetFriendlyMemberPath(targetMember, configInfo.TargetType.GetFriendlyName(), RootTargetMemberName);

        public static string GetFriendlyMemberPath(this IQualifiedMember member, IQualifiedMember rootMember)
            => GetFriendlyMemberPath(member, rootMember.GetFriendlyTypeName(), rootMember.Name);

        public static string GetFriendlyMemberPath(
            this IQualifiedMember member,
            string rootTypeName,
            string rootMemberName)
        {
            var memberPath = member.GetPath();

            if (memberPath == rootMemberName)
            {
                return rootTypeName;
            }

            if (memberPath.StartsWith(rootMemberName, Ordinal))
            {
                return rootTypeName + memberPath.Substring(rootMemberName.Length);
            }

            var rootMemberNameIndex = memberPath.IndexOf("." + rootMemberName + ".", Ordinal);

            if (rootMemberNameIndex == -1)
            {
                return rootTypeName + memberPath;
            }

            var rootMemberString = memberPath.Substring(rootMemberNameIndex + rootMemberName.Length + 2);
            var path = rootTypeName + "." + rootMemberString;

            return path;
        }

        public static bool IsEntityId(this Member member) => member.MemberInfo?.HasKeyAttribute() == true;

        public static bool IsUnmappable(this QualifiedMember member, out string reason)
        {
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

            return member.MemberChain[member.Depth - 2].Type.IsValueType();
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
        public static bool IsEnumerableElement(this QualifiedMember member) => member.LeafMember.IsEnumerableElement();

        [DebuggerStepThrough]
        public static bool IsEnumerableElement(this Member member) => member.MemberType == MemberType.EnumerableElement;

        [DebuggerStepThrough]
        public static bool IsConstructorParameter(this QualifiedMember member)
            => member.LeafMember.MemberType == MemberType.ConstructorParameter;

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

                if (member.Equals(otherMembersLeafMember))
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
            => Expression.Call(instance, targetMember.Name, EmptyTypeArray, value);

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
        public static QualifiedMember ToSourceMember(
            this Expression memberAccess,
            MapperContext mapperContext,
            Action<ExpressionType> nonMemberAction = null)
        {
            return CreateMember(
                memberAccess,
                RootSource,
                GlobalContext.Instance.MemberCache.GetSourceMembers,
                nonMemberAction ?? ThrowIfUnsupported,
                mapperContext);
        }

        public static QualifiedMember ToSourceMemberOrNull(
            this LambdaExpression memberAccess,
            MapperContext mapperContext,
            out string failureReason)
        {
            var hasUnsupportedNodeType = false;
            var sourceMember = memberAccess.ToSourceMember(mapperContext, nt => hasUnsupportedNodeType = true);

            if (hasUnsupportedNodeType)
            {
                failureReason = $"Unable to determine source member from '{memberAccess.Body.ToReadableString()}'";
                return null;
            }

            if (sourceMember == null)
            {
                failureReason = $"Source member {memberAccess.Body.ToReadableString()} is not readable";
                return null;
            }

            failureReason = null;
            return sourceMember;
        }

        public static QualifiedMember ToTargetMemberOrNull(
            this LambdaExpression memberAccess,
            Type targetType,
            MapperContext mapperContext,
            out string failureReason)
        {
            var hasUnsupportedNodeType = false;
            var targetMember = memberAccess.ToTargetMember(mapperContext, nt => hasUnsupportedNodeType = true);

            if (hasUnsupportedNodeType)
            {
                failureReason = $"Unable to determine target member from '{memberAccess.Body.ToReadableString()}'";
                return null;
            }

            if (targetMember == null)
            {
                var targetPath = memberAccess
                    .ToSourceMember(mapperContext)?
                    .GetFriendlyMemberPath(targetType.GetFriendlyName(), RootTargetMemberName) ??
                     memberAccess.Body.ToReadableString();

                failureReason = $"Target member {targetPath} is not writeable";
                return null;
            }

            if (targetMember.IsUnmappable(out var reason))
            {
                var targetPath = targetMember.GetFriendlyMemberPath(targetType.GetFriendlyName(), RootTargetMemberName);
                failureReason = $"Target member {targetPath} is not mappable ({reason})";
                return null;
            }

            failureReason = null;
            return targetMember;
        }

#if NET35
        public static QualifiedMember ToTargetMember(this LinqExp.LambdaExpression memberAccess, MapperContext mapperContext)
            => memberAccess.ToDlrExpression().ToTargetMember(mapperContext);

        public static QualifiedMember ToTargetMember(
            this LinqExp.LambdaExpression memberAccess,
            MapperContext mapperContext,
            Action<ExpressionType> nonMemberAction)
        {
            return memberAccess.ToDlrExpression().ToTargetMember(mapperContext, nonMemberAction);
        }
#endif
        public static QualifiedMember ToTargetMember(
            this LambdaExpression memberAccess,
            MapperContext mapperContext,
            Action<ExpressionType> nonMemberAction = null)
        {
            return CreateMember(
                memberAccess.Body,
                RootTarget,
                GlobalContext.Instance.MemberCache.GetTargetMembers,
                nonMemberAction ?? ThrowIfUnsupported,
                mapperContext);
        }

        private static void ThrowIfUnsupported(ExpressionType unsupportedNodeType) =>
            throw new NotSupportedException($"Unable to get member access from {unsupportedNodeType} Expression");

        private static QualifiedMember CreateMember(
            Expression memberAccessExpression,
            Func<Type, Member> rootMemberFactory,
            Func<Type, IList<Member>> membersFactory,
            Action<ExpressionType> nonMemberAction,
            MapperContext mapperContext)
        {
            var memberAccesses = memberAccessExpression.GetMemberAccessChain(nonMemberAction, out var rootExpression);

            if (memberAccesses.None())
            {
                return null;
            }

            var rootMember = rootMemberFactory.Invoke(rootExpression.Type);
            var parentMember = rootMember;

            var memberChain = new Member[memberAccesses.Count + 1];
            memberChain[0] = rootMember;

            for (var i = 0; i < memberAccesses.Count;)
            {
                var memberAccess = memberAccesses[i++];
                var memberName = GetMemberName(memberAccess);
                var members = membersFactory.Invoke(parentMember.Type);
                var member = members.FirstOrDefault(memberName, (mn, m) => m.Name == mn);

                if (member == null)
                {
                    return null;
                }

                memberChain[i] = member;
                parentMember = member;
            }

            return QualifiedMember.From(memberChain, mapperContext);
        }

        public static IList<Expression> GetMemberAccessChain(
            this Expression memberAccessExpression,
            Action<ExpressionType> nonMemberAction,
            out Expression rootExpression)
        {
            rootExpression = memberAccessExpression;
            var memberAccesses = new List<Expression>();

            while (rootExpression.NodeType != ExpressionType.Parameter)
            {
                var memberExpression = GetMemberAccess(rootExpression, nonMemberAction);

                if (memberExpression == null)
                {
                    return Enumerable<Expression>.EmptyArray;
                }

                memberAccesses.Insert(0, memberExpression);
                rootExpression = memberExpression.GetParentOrNull();

                if (rootExpression == null)
                {
                    return Enumerable<Expression>.EmptyArray;
                }
            }

            AdjustMemberAccessesIfRootedInMappingData(memberAccesses, ref rootExpression);

            return memberAccesses;
        }

        private static Expression GetMemberAccess(Expression expression, Action<ExpressionType> nonMemberAction)
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

                nonMemberAction.Invoke(expression.NodeType);
                return null;
            }
        }

        private static void AdjustMemberAccessesIfRootedInMappingData(IList<Expression> memberAccesses, ref Expression expression)
        {
            if (!expression.Type.IsClosedTypeOf(typeof(IMappingData<,>)) &&
                !expression.Type.IsClosedTypeOf(typeof(IObjectMappingData<,>)))
            {
                return;
            }

            var mappingDataRoot = memberAccesses[0];

            if (mappingDataRoot.NodeType != ExpressionType.MemberAccess)
            {
                return;
            }

            expression = Parameters.Create(mappingDataRoot.Type);

            memberAccesses.RemoveAt(0);

            for (var i = 0; i < memberAccesses.Count; i++)
            {
                memberAccesses[i] = memberAccesses[i].Replace(mappingDataRoot, expression);
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
