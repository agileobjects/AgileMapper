namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using static Members.Member;
    using static ParametersSwapper;

    internal class ValuesInjector
    {
        public static ValuesInjector For(LambdaExpression lambda)
        {
            // Lambda has parameters - either ctx, (s, t), (s, t, i), etc
            // ctx members or multi-param lambda parameters may or may not be used
            // 1. find parameter accesses in lambda body
            // 2. create dictionary of used parameters and funcs to get the appropriate
            //    value for each from a given mapper data

            RequiredValuesSet requiredValues;

            switch (lambda.Parameters.Count)
            {
                case 1:
                    requiredValues = MappingContextMemberAccessFinder.GetValuesRequiredBy(lambda);
                    break;

                default:
                    return null;
            }

            var requiredValuesCount = requiredValues.GetRequiredValuesCount();

            switch (requiredValuesCount)
            {
                case 0:
                case 1:

                    break;
            }
        }

        [Flags]
        private enum RequiredValue
        {
            Undefined = 0,
            MappingContext = 1,
            Parent = 2,
            Source = 4,
            Target = 8,
            ElementIndex = 16,
            ElementKey = 32
        }

        private class RequiredValuesSet
        {
            private int? _requiredValuesCount;
            private RequiredValue _requiredValues;
            private Expression _mappingContext;
            private Expression _parent;
            private Expression _source;
            private Expression _target;
            private Expression _elementIndex;
            private Expression _elementKey;

            public Expression MappingContext
            {
                get => _mappingContext;
                set
                {
                    if (AssignIfMissing(RequiredValue.MappingContext))
                    {
                        _mappingContext = value;
                    }
                }
            }

            public Expression Parent
            {
                get => _parent;
                set
                {
                    if (AssignIfMissing(RequiredValue.Parent))
                    {
                        _parent = value;
                    }
                }
            }

            public Expression Source
            {
                get => _source;
                set
                {
                    if (AssignIfMissing(RequiredValue.Source))
                    {
                        _source = value;
                    }
                }
            }

            public Expression Target
            {
                get => _target;
                set
                {
                    if (AssignIfMissing(RequiredValue.Target))
                    {
                        _target = value;
                    }
                }
            }

            public Expression ElementIndex
            {
                get => _elementIndex;
                set
                {
                    if (AssignIfMissing(RequiredValue.ElementIndex))
                    {
                        _elementIndex = value;
                    }
                }
            }

            public Expression ElementKey
            {
                get => _elementKey;
                set
                {
                    if (AssignIfMissing(RequiredValue.ElementKey))
                    {
                        _elementKey = value;
                    }
                }
            }

            public IValueReplacer GetValueReplacer()
            {
                switch (GetRequiredValuesCount())
                {
                    case 0:
                        return null;
                    
                    case 1:
                        return new SingleValueReplacer();

                    default:
                        return null;
                }
            }

            private int GetRequiredValuesCount()
            {
                if (_requiredValues == RequiredValue.Undefined)
                {
                    return 0;
                }

                if (_requiredValuesCount.HasValue)
                {
                    return _requiredValuesCount.Value;
                }

                // See https://stackoverflow.com/questions/677204/counting-the-number-of-flags-set-on-an-enumeration
                var count = 0;
                var value = (int)_requiredValues;

                while (value != 0)
                {
                    value &= (value - 1);
                    ++count;
                }

                _requiredValuesCount = count;
                return count;
            }

            private KeyValuePair<Expression, ValueFactory> GetSingleValueAndFactory()
            {
                if (Has(RequiredValue.MappingContext))
                {
                    return new KeyValuePair<Expression, ValueFactory>(MappingContext, );
                }
            }

            private bool AssignIfMissing(RequiredValue requiredValue)
            {
                if (Has(requiredValue))
                {
                    return false;
                }

                _requiredValues |= requiredValue;
                return true;
            }

            private bool Has(RequiredValue requiredValue)
                => (_requiredValues & requiredValue) == requiredValue;
        }

        private class MappingContextMemberAccessFinder : ExpressionVisitor
        {
            private readonly ParameterExpression _contextParameter;
            private readonly RequiredValuesSet _requiredValues;

            private MappingContextMemberAccessFinder(ParameterExpression contextParameter)
            {
                _contextParameter = contextParameter;
                _requiredValues = new RequiredValuesSet();
            }

            public static RequiredValuesSet GetValuesRequiredBy(LambdaExpression lambda)
            {
                var finder = new MappingContextMemberAccessFinder(lambda.Parameters[0]);
                finder.Visit(lambda.Body);

                return finder._requiredValues;
            }


            protected override Expression VisitMethodCall(MethodCallExpression methodCall)
            {
                if (methodCall.Object == _contextParameter)
                {
                    _requiredValues.MappingContext = _contextParameter;
                }

                return base.VisitMethodCall(methodCall);
            }

            protected override Expression VisitMember(MemberExpression memberAccess)
            {
                if (memberAccess.Expression != _contextParameter)
                {
                    return base.VisitMember(memberAccess);
                }

                switch (memberAccess.Member.Name)
                {
                    case nameof(IMappingData.Parent):
                        _requiredValues.Parent = memberAccess;
                        break;

                    case RootSourceMemberName:
                        _requiredValues.Source = memberAccess;
                        break;

                    case RootTargetMemberName:
                        _requiredValues.Target = memberAccess;
                        break;

                    case nameof(IMappingData<int, int>.ElementIndex):
                        _requiredValues.ElementIndex = memberAccess;
                        break;

                    case nameof(IMappingData<int, int>.ElementKey):
                        _requiredValues.ElementKey = memberAccess;
                        break;
                }

                return base.VisitMember(memberAccess);
            }
        }

        private interface IValueReplacer
        {
            Expression Replace(LambdaExpression lambda, SwapArgs swapArgs);
        }

        private delegate Expression ValueFactory(SwapArgs swapArgs);

        private class SingleValueReplacer
        {
            private readonly Expression _target;
            private readonly ValueFactory _valueFactory;

            public SingleValueReplacer(Expression target, ValueFactory valueFactory)
            {
                _target = target;
                _valueFactory = valueFactory;
            }

            public Expression Replace(LambdaExpression lambda, SwapArgs swapArgs)
            {
                var value = _valueFactory.Invoke(swapArgs);

                return lambda.Body.Replace(_target, value);
            }
        }
    }
}