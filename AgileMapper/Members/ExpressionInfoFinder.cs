namespace AgileObjects.AgileMapper.Members
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using TypeConversion;
#if NET35
    using Microsoft.Scripting.Ast;
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using System.Linq.Expressions;
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static Member;

    internal class ExpressionInfoFinder
    {
        public static readonly ExpressionInfo EmptyExpressionInfo =
            new ExpressionInfo(null, Enumerable<Expression>.EmptyArray);

        private readonly Expression _mappingDataObject;

        public ExpressionInfoFinder(Expression mappingDataObject)
        {
            _mappingDataObject = mappingDataObject;
        }

        public ExpressionInfo FindIn(Expression expression, bool targetCanBeNull)
        {
            var finder = new ExpressionInfoFinderInstance(_mappingDataObject, targetCanBeNull);
            var info = finder.FindIn(expression);

            return info;
        }

        private class ExpressionInfoFinderInstance : ExpressionVisitor
        {
            private readonly Expression _mappingDataObject;
            private readonly bool _includeTargetNullChecking;
            private readonly ICollection<Expression> _stringMemberAccessSubjects;
            private readonly ICollection<Expression> _allInvocations;
            private readonly ICollection<Expression> _multiInvocations;
            private readonly ICollection<string> _nullCheckSubjects;
            private readonly Dictionary<string, Expression> _nestedAccessesByPath;

            public ExpressionInfoFinderInstance(Expression mappingDataObject, bool targetCanBeNull)
            {
                _mappingDataObject = mappingDataObject;
                _includeTargetNullChecking = targetCanBeNull;
                _stringMemberAccessSubjects = new List<Expression>();
                _allInvocations = new List<Expression>();
                _multiInvocations = new List<Expression>();
                _nullCheckSubjects = new List<string>();
                _nestedAccessesByPath = new Dictionary<string, Expression>();
            }

            public ExpressionInfo FindIn(Expression expression)
            {
                Visit(expression);

                if (_nestedAccessesByPath.None() && _multiInvocations.None())
                {
                    return EmptyExpressionInfo;
                }

                var nestedAccessChecks = GetNestedAccessChecks();

                var multiInvocations = _multiInvocations.Any()
                    ? _multiInvocations.OrderBy(inv => inv.ToString()).ToArray()
                    : Enumerable<Expression>.EmptyArray;

                return new ExpressionInfo(nestedAccessChecks, multiInvocations);
            }

            private Expression GetNestedAccessChecks()
            {
                if (_nestedAccessesByPath.None())
                {
                    return null;
                }

                return _nestedAccessesByPath
                    .Values
                    .Reverse()
                    .Project(GetAccessCheck)
                    .Aggregate(
                        default(Expression),
                        (accessChecksSoFar, accessCheck) => (accessChecksSoFar != null)
                            ? Expression.AndAlso(accessChecksSoFar, accessCheck)
                            : accessCheck);
            }

            private static Expression GetAccessCheck(Expression access)
            {
                Expression count;

                switch (access.NodeType)
                {
                    case ArrayIndex:
                        count = Expression.ArrayLength(((BinaryExpression)access).Left);
                        break;

                    case Index:
                        count = ((IndexExpression)access).Object.GetCount();
                        break;

                    default:
                        return access.GetIsNotDefaultComparison();
                }

                return Expression.GreaterThan(count, ToNumericConverter<int>.Zero);
            }

            protected override Expression VisitBinary(BinaryExpression binary)
            {
                if (TryGetNullComparison(binary.Left, binary.Right, binary, out var comparedValue) ||
                    TryGetNullComparison(binary.Right, binary.Left, binary, out comparedValue))
                {
                    AddExistingNullCheck(comparedValue);

                    return base.VisitBinary(binary);
                }

                if (IsRelevantArrayIndexAccess(binary))
                {
                    AddMemberAccess(binary);
                }

                return base.VisitBinary(binary);
            }

            private bool TryGetNullComparison(
                Expression comparedOperand,
                Expression nullOperand,
                Expression binary,
                out Expression comparedValue)
            {
                if ((binary.NodeType != Equal) && (binary.NodeType != NotEqual))
                {
                    comparedValue = null;
                    return false;
                }

                if ((nullOperand.NodeType != Constant) || (((ConstantExpression)nullOperand).Value != null))
                {
                    comparedValue = null;
                    return false;
                }

                comparedValue = GuardMemberAccess(comparedOperand) ? comparedOperand : null;
                return comparedValue != null;
            }

            private static bool IsRelevantArrayIndexAccess(BinaryExpression binary)
            {
                return (binary.NodeType == ArrayIndex) &&
                       (binary.Right.NodeType != Parameter);
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                if (IsRootObject(memberAccess))
                {
                    return base.VisitMember(memberAccess);
                }

                if (IsNullableHasValueAccess(memberAccess))
                {
                    AddExistingNullCheck(memberAccess.Expression);
                }

                AddStringMemberAccessSubjectIfAppropriate(memberAccess.Expression);
                AddMemberAccessIfAppropriate(memberAccess);

                return base.VisitMember(memberAccess);
            }

            private bool IsRootObject(MemberExpression memberAccess) => !IsNotRootObject(memberAccess);

            private bool IsNotRootObject(MemberExpression memberAccess)
            {
                if (memberAccess.Member.Name == "Parent")
                {
                    // ReSharper disable once PossibleNullReferenceException
                    return !memberAccess.Member.DeclaringType.Name
                        .StartsWith(nameof(IMappingData), StringComparison.Ordinal);
                }

                if (memberAccess.Expression != _mappingDataObject)
                {
                    return true;
                }

                if (memberAccess.Member.Name == "EnumerableIndex")
                {
                    return false;
                }

                if (memberAccess.Member.Name == RootSourceMemberName)
                {
                    return false;
                }

                return _includeTargetNullChecking || (memberAccess.Member.Name != RootTargetMemberName);
            }

            private static bool IsNullableHasValueAccess(MemberExpression memberAccess)
            {
                return (memberAccess.Expression != null) &&
                       (memberAccess.Member.Name == "HasValue") &&
                       (memberAccess.Expression.Type.IsNullableType());
            }

            protected override Expression VisitIndex(IndexExpression indexAccess)
            {
                if ((indexAccess.Object.Type != typeof(string)) &&
                     !indexAccess.Object.Type.IsDictionary() &&
                     IndexDoesNotUseParameter(indexAccess.Arguments[0]))
                {
                    AddMemberAccess(indexAccess);
                }

                return base.VisitIndex(indexAccess);
            }

            private static bool IndexDoesNotUseParameter(Expression indexExpression)
            {
                if (indexExpression == null)
                {
                    return true;
                }

                switch (indexExpression.NodeType)
                {
                    case Call:
                        var methodCall = (MethodCallExpression)indexExpression;

                        return IndexDoesNotUseParameter(methodCall.Object) &&
                               methodCall.Arguments.All(IndexDoesNotUseParameter);

                    case Parameter:
                        return false;
                }

                return true;
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if ((methodCall.Object != _mappingDataObject) &&
                    (methodCall.Method.DeclaringType != typeof(IMappingData)))
                {
                    if (IsNullableGetValueOrDefaultCall(methodCall))
                    {
                        AddExistingNullCheck(methodCall.Object);
                    }

                    AddStringMemberAccessSubjectIfAppropriate(methodCall.Object);
                    AddInvocationIfNecessary(methodCall);
                    AddMemberAccessIfAppropriate(methodCall);
                }

                return base.VisitMethodCall(methodCall);
            }

            private static bool IsNullableGetValueOrDefaultCall(MethodCallExpression methodCall)
            {
                return (methodCall.Object != null) &&
                       (methodCall.Method.Name == "GetValueOrDefault") &&
                       (methodCall.Object.Type.IsNullableType());
            }

            private void AddExistingNullCheck(Expression checkedAccess)
            {
                _nullCheckSubjects.Add(checkedAccess.ToString());
            }

            private void AddStringMemberAccessSubjectIfAppropriate(Expression member)
            {
                if ((member?.Type == typeof(string)) && AccessSubjectCouldBeNull(member))
                {
                    _stringMemberAccessSubjects.Add(member);
                }
            }

            private bool AccessSubjectCouldBeNull(Expression expression)
            {
                Expression subject;
                MemberExpression memberAccess;

                if (expression.NodeType == MemberAccess)
                {
                    memberAccess = (MemberExpression)expression;
                    subject = memberAccess.Expression;
                }
                else
                {
                    memberAccess = null;
                    subject = ((MethodCallExpression)expression).Object;
                }

                if (subject == null)
                {
                    return false;
                }

                if (subject.Type.CannotBeNull())
                {
                    return false;
                }

                return (expression.NodeType != MemberAccess) ||
                       IsNotRootObject(memberAccess);
            }

            protected override Expression VisitInvocation(InvocationExpression invocation)
            {
                AddInvocationIfNecessary(invocation);

                return base.VisitInvocation(invocation);
            }

            private void AddInvocationIfNecessary(Expression invocation)
            {
                if (!_allInvocations.Contains(invocation))
                {
                    _allInvocations.Add(invocation);
                }
                else if (!_multiInvocations.Contains(invocation))
                {
                    _multiInvocations.Add(invocation);
                }
            }

            private void AddMemberAccessIfAppropriate(Expression memberAccess)
            {
                if (GuardMemberAccess(memberAccess))
                {
                    AddMemberAccess(memberAccess);
                }
            }

            private bool GuardMemberAccess(Expression memberAccess)
            {
                if (IsNonNullReturnMethodCall(memberAccess))
                {
                    return false;
                }

                if (memberAccess.Type.CannotBeNull() || !memberAccess.IsRootedIn(_mappingDataObject))
                {
                    return false;
                }

                if ((memberAccess.Type == typeof(string)) && !_stringMemberAccessSubjects.Contains(memberAccess))
                {
                    return false;
                }

                return true;
            }

            private static bool IsNonNullReturnMethodCall(Expression memberAccess)
            {
                if (memberAccess.NodeType != Call)
                {
                    return false;
                }

                var method = ((MethodCallExpression)memberAccess).Method;

                switch (method.Name)
                {
                    case "ToString" when method.DeclaringType == typeof(object):
                    case "Split" when method.DeclaringType == typeof(string):
                    case "GetEnumerator" when method.DeclaringType.IsClosedTypeOf(typeof(IEnumerable<>)):
                        return true;

                    case "Select":
                    case "SelectMany":
                    case "OrderBy":
                    case "OrderByDescending":
                    case "ToList":
                    case "ToArray":
                    case "Where":
                        return method.DeclaringType == typeof(Enumerable);

                    default:
                        return false;
                }
            }

            private void AddMemberAccess(Expression memberAccess)
            {
                var memberAccessString = memberAccess.ToString();

                if (_nullCheckSubjects.Contains(memberAccessString) ||
                    _nestedAccessesByPath.ContainsKey(memberAccessString))
                {
                    return;
                }

                _nestedAccessesByPath.Add(memberAccessString, memberAccess);
            }
        }

        public class ExpressionInfo
        {
            public ExpressionInfo(
                Expression nestedAccessChecks,
                IList<Expression> multiInvocations)
            {
                NestedAccessChecks = nestedAccessChecks;
                MultiInvocations = multiInvocations;
            }

            public Expression NestedAccessChecks { get; }

            public IList<Expression> MultiInvocations { get; }
        }
    }
}