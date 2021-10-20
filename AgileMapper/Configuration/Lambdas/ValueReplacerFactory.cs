namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
    using System;
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ObjectPopulation;
    using static LambdaValue;

    internal class ValueReplacerFactory
    {
        private delegate bool ApplicabilityPredicate(Type[] contextTypes, Type[] funcArguments);

        private static readonly ValueReplacerFactory _empty = new();

        private static readonly ValueReplacerFactory[] _factories =
        {
            new(MappingContext, IsContext),
            new(Source, IsSourceOnly),
            new(Source | Target),
            new(Source | Target | ElementIndex),
            new(Source | Target | CreatedObject),
            new(Source | Target | CreatedObject | ElementIndex)
        };

        private readonly LambdaValue _lambdaValue;
        private readonly ApplicabilityPredicate _applicabilityPredicate;
        private readonly int _numberOfParameters;

        private ValueReplacerFactory(
            LambdaValue lambdaValue,
            ApplicabilityPredicate applicabilityPredicate = null)
            : this(applicabilityPredicate)
        {
            _lambdaValue = lambdaValue;
            _numberOfParameters = lambdaValue.Count();
        }

        private ValueReplacerFactory(ApplicabilityPredicate applicabilityPredicate = null)
        {
            _applicabilityPredicate = applicabilityPredicate ?? MatchesLambdaValue;
        }

        #region Factory Method

        public static ValueReplacerFactory For(Type[] contextTypes, Type[] funcArguments)
        {
            var funcArgumentCount = funcArguments.Length;

            if (funcArgumentCount == 0)
            {
                return _empty;
            }

            foreach (var injectorFactory in _factories)
            {
                if (injectorFactory._numberOfParameters > funcArgumentCount)
                {
                    break;
                }

                if (injectorFactory.AppliesTo(contextTypes, funcArguments))
                {
                    return injectorFactory;
                }
            }

            return null;
        }

        #endregion

        #region Applicability

        private bool AppliesTo(Type[] contextTypes, Type[] funcArguments)
        {
            return (funcArguments.Length == _numberOfParameters) &&
                   _applicabilityPredicate.Invoke(contextTypes, funcArguments);
        }

        private static bool IsContext(Type[] contextTypes, Type[] funcArguments)
        {
            var contextTypeArgument = funcArguments[0];

            if (!contextTypeArgument.IsGenericType())
            {
                return false;
            }

            if (Is(typeof(IMappingData<,>), contextTypeArgument, ref funcArguments))
            {
                return Is(Source | Target, contextTypes, funcArguments);
            }

            if (Is(typeof(IObjectCreationMappingData<,,>), contextTypeArgument, ref funcArguments))
            {
                return Is(Source | Target | CreatedObject, contextTypes, funcArguments);
            }

            return false;
        }

        private static bool Is(Type contextType, Type contextTypeArgument, ref Type[] funcArguments)
        {
            if (contextTypeArgument.GetGenericTypeDefinition() != contextType)
            {
                return false;
            }

            funcArguments = contextTypeArgument.GetGenericTypeArguments();
            return true;

        }

        private static bool IsSourceOnly(Type[] contextTypes, Type[] funcArguments)
            => (contextTypes.Length == 1) && Is(Source, contextTypes, funcArguments);

        private bool MatchesLambdaValue(Type[] contextTypes, Type[] funcArguments)
            => Is(_lambdaValue, contextTypes, funcArguments);

        private static bool Is(LambdaValue lambdaValue, IList<Type> contextTypes, IList<Type> funcArguments)
        {
            if (lambdaValue.Has(Source) && !contextTypes[0].IsAssignableTo(funcArguments[0]))
            {
                return false;
            }

            if (lambdaValue.Has(Target) && !contextTypes[1].IsAssignableTo(funcArguments[1]))
            {
                return false;
            }

            if (lambdaValue.Has(ElementIndex) && funcArguments.Last() != typeof(int?))
            {
                return false;
            }

            if (lambdaValue.Has(CreatedObject) && !contextTypes[2].IsAssignableTo(funcArguments[2]))
            {
                return false;
            }

            return true;
        }

        #endregion

        public IValueReplacer CreateFor(LambdaExpression lambda, MappingConfigInfo configInfo)
        {
            switch (_lambdaValue)
            {
                case default(LambdaValue):
                    return new NullValueReplacer(lambda);

                case MappingContext:
                    return MappingContextValueReplacer.CreateFor(lambda, configInfo);

                default:
                    return ContextValuesValueReplacer.Create(lambda, configInfo);
            }
        }
    }
}