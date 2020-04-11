namespace AgileObjects.AgileMapper.Members.MemberExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
#if NET35
    using static Microsoft.Scripting.Ast.ExpressionType;
#else
    using static System.Linq.Expressions.ExpressionType;
#endif
    using static Member;

    internal class NestedAccessChecksFactory : ExpressionVisitor
    {
        private readonly Expression _rootMappingData;
        private readonly bool _invertChecks;
        private ICollection<Expression> _stringMemberAccessSubjects;
        private ICollection<string> _nullCheckSubjects;
        private Dictionary<string, Expression> _nestedAccessesByPath;

        private NestedAccessChecksFactory(IMemberMapperData mapperData, bool invertChecks)
        {
            _rootMappingData = mapperData?.RootMappingDataObject;
            _invertChecks = invertChecks;
        }

        public static Expression GetNestedAccessChecksFor(
            Expression expression,
            IMemberMapperData mapperData = null,
            bool invertChecks = false)
        {
            var factory = new NestedAccessChecksFactory(mapperData, invertChecks);
            var checks = factory.CreateFor(expression);
            return checks;
        }

        private ICollection<Expression> StringMemberAccessSubjects
            => _stringMemberAccessSubjects ??= new List<Expression>();

        private ICollection<string> NullCheckSubjects
            => _nullCheckSubjects ??= new List<string>();

        private Dictionary<string, Expression> NestedAccessesByPath
            => _nestedAccessesByPath ??= new Dictionary<string, Expression>();

        public Expression CreateFor(Expression expression)
        {
            Visit(expression);

            return (_nestedAccessesByPath != null) ? GetNestedAccessChecks() : null;
        }

        private Expression GetNestedAccessChecks()
        {
            if (_nestedAccessesByPath == null)
            {
                return null;
            }

            var nestedAccessCount = _nestedAccessesByPath.Count;

            if (nestedAccessCount == 1)
            {
                return GetAccessCheck(_nestedAccessesByPath.Values.First());
            }

            var nestedAccessCheckChain = default(Expression);

            foreach (var nestedAccessCheck in _nestedAccessesByPath.Values.Reverse().Project(GetAccessCheck))
            {
                if (nestedAccessCheckChain == null)
                {
                    nestedAccessCheckChain = nestedAccessCheck;
                    continue;
                }

                nestedAccessCheckChain = _invertChecks
                    ? Expression.OrElse(nestedAccessCheckChain, nestedAccessCheck)
                    : Expression.AndAlso(nestedAccessCheckChain, nestedAccessCheck);
            }

            return nestedAccessCheckChain;
        }

        private Expression GetAccessCheck(Expression access)
        {
            switch (access.NodeType)
            {
                case ArrayIndex:
                    var arrayIndexAccess = (BinaryExpression)access;
                    var arrayLength = Expression.ArrayLength(arrayIndexAccess.Left);
                    var arrayIndexValue = arrayIndexAccess.Right;

                    return _invertChecks
                        ? Expression.Equal(arrayLength, arrayIndexValue)
                        : Expression.GreaterThan(arrayLength, arrayIndexValue);

                case Index:
                    var index = (IndexExpression)access;
                    var indexKeyType = index.Indexer.GetGetter().GetParameters().First().ParameterType;

                    if (!indexKeyType.IsNumeric())
                    {
                        goto default;
                    }

                    var count = index.Object.GetCount();

                    if (count == null)
                    {
                        goto default;
                    }

                    var indexValue = index.Arguments.First().GetConversionTo(count.Type);

                    return _invertChecks
                        ? Expression.LessThanOrEqual(count, indexValue)
                        : Expression.GreaterThan(count, indexValue);

                default:
                    return _invertChecks
                        ? access.GetIsDefaultComparison()
                        : access.GetIsNotDefaultComparison();
            }
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
            if ((memberAccess.Expression == null) || IsRootObject(memberAccess))
            {
                return base.VisitMember(memberAccess);
            }

            if (memberAccess.IsNullableHasValueAccess())
            {
                AddExistingNullCheck(memberAccess.Expression);
            }

            AddStringMemberAccessSubjectIfAppropriate(memberAccess.Expression);
            AddMemberAccessIfAppropriate(memberAccess);

            return base.VisitMember(memberAccess);
        }

        private bool IsNotRootObject(MemberExpression memberAccess) => !IsRootObject(memberAccess);

        private bool IsRootObject(MemberExpression memberAccess)
        {
            if (memberAccess.Member.Name == nameof(IMappingData.Parent))
            {
                // ReSharper disable once PossibleNullReferenceException
                return memberAccess.Member.DeclaringType.Name
                    .StartsWith(nameof(IMappingData), StringComparison.Ordinal);
            }

            if (memberAccess.Expression != _rootMappingData)
            {
                return false;
            }

            switch (memberAccess.Member.Name)
            {
                case nameof(IMappingData<int, int>.ElementIndex):
                case RootSourceMemberName:
                case RootTargetMemberName:
                    return true;

                default:
                    return false;
            }
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
            if (methodCall.IsMappingDataObjectCall(_rootMappingData))
            {
                return base.VisitMethodCall(methodCall);
            }

            if (IsNullableGetValueOrDefaultCall(methodCall))
            {
                AddExistingNullCheck(methodCall.Object);
            }

            AddStringMemberAccessSubjectIfAppropriate(methodCall.Object);
            AddMemberAccessIfAppropriate(methodCall);

            return base.VisitMethodCall(methodCall);
        }

        private static bool IsNullableGetValueOrDefaultCall(MethodCallExpression methodCall)
        {
            return (methodCall.Object != null) &&
                   (methodCall.Method.Name == nameof(Nullable<int>.GetValueOrDefault)) &&
                   (methodCall.Object.Type.IsNullableType());
        }

        private void AddExistingNullCheck(Expression checkedAccess)
        {
            NullCheckSubjects.Add(checkedAccess.ToString());
        }

        private void AddStringMemberAccessSubjectIfAppropriate(Expression member)
        {
            if ((member?.Type == typeof(string)) && AccessSubjectCouldBeNull(member))
            {
                StringMemberAccessSubjects.Add(member);
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

            return (expression.NodeType != MemberAccess) || IsNotRootObject(memberAccess);
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

            if (memberAccess.Type.CannotBeNull() ||
             ((_rootMappingData != null) && !memberAccess.IsRootedIn(_rootMappingData)))
            {
                return false;
            }

            if ((memberAccess.Type == typeof(string)) &&
               (_stringMemberAccessSubjects?.Contains(memberAccess) != true))
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
                case nameof(string.ToString) when method.DeclaringType == typeof(object):
                case nameof(string.Split) when method.DeclaringType == typeof(string):
                case nameof(IEnumerable<int>.GetEnumerator) when method.DeclaringType.IsClosedTypeOf(typeof(IEnumerable<>)):
                    return true;

                case nameof(Enumerable.Select):
                case nameof(Enumerable.SelectMany):
                case nameof(PublicEnumerableExtensions.Project):
                case nameof(PublicEnumerableExtensions.Filter):
                case nameof(Enumerable.Where):
                case nameof(Enumerable.OrderBy):
                case nameof(Enumerable.OrderByDescending):
                case nameof(Enumerable.ToList):
                case nameof(Enumerable.ToArray):
                    return (method.DeclaringType == typeof(Enumerable)) ||
                           (method.DeclaringType == typeof(PublicEnumerableExtensions));

                default:
                    return false;
            }
        }

        private void AddMemberAccess(Expression memberAccess)
        {
            var memberAccessString = memberAccess.ToString();

            if (_nullCheckSubjects?.Contains(memberAccessString) == true ||
                _nestedAccessesByPath?.ContainsKey(memberAccessString) == true)
            {
                return;
            }

            NestedAccessesByPath.Add(memberAccessString, memberAccess);
        }
    }
}