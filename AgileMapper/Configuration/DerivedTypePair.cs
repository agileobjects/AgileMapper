namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using System.Linq.Expressions;
    using Members;

    internal class DerivedTypePair : UserConfiguredItemBase
    {
        private readonly Type _derivedSourceType;
        private readonly Func<IMappingData, bool> _derivedTypePredicate;

        private DerivedTypePair(
            MappingConfigInfo configInfo,
            Type derivedSourceType,
            Type derivedTargetType,
            Func<IMappingData, bool> derivedTypePredicate)
            : base(configInfo)
        {
            _derivedSourceType = derivedSourceType;
            DerivedTargetType = derivedTargetType;
            _derivedTypePredicate = derivedTypePredicate;
        }

        public static DerivedTypePair For<TSource, TDerivedSource, TTarget, TDerivedTarget>(MappingConfigInfo configInfo)
        {
            var derivedTypePredicate = GetDerivedTypePredicateOrNull<TSource, TTarget>(configInfo);

            return new DerivedTypePair(
                configInfo.ForTargetType<TTarget>(),
                typeof(TDerivedSource),
                typeof(TDerivedTarget),
                derivedTypePredicate);
        }

        private static Func<IMappingData, bool> GetDerivedTypePredicateOrNull<TSource, TTarget>(MappingConfigInfo configInfo)
        {
            var condition = configInfo.GetConditionOrNull<TSource, TTarget>();

            if (condition == null)
            {
                return null;
            }

            var conditionLambda = Expression.Lambda<Func<IMappingData, bool>>(condition, Parameters.MappingData);
            var conditionPredicate = conditionLambda.Compile();

            return conditionPredicate;
        }

        public Type DerivedTargetType { get; }

        public bool AppliesTo(IBasicMappingData mappingData)
        {
            if (_derivedSourceType != mappingData.SourceType)
            {
                return false;
            }

            if (base.AppliesTo(mappingData.MapperData))
            {
                return (_derivedTypePredicate == null) || _derivedTypePredicate.Invoke(mappingData);
            }

            return false;
        }
    }
}