namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;
    using Members;

    internal class ObjectMapperFactory
    {
        private readonly ICache<ObjectMapperKey, object> _cache;

        public ObjectMapperFactory(GlobalContext globalContext)
        {
            _cache = globalContext.CreateCache<ObjectMapperKey, object>();
        }

        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMappingContext omc)
        {
            var mapper = (IObjectMapper<TTarget>)_cache.GetOrAdd(new ObjectMapperKey(omc), k =>
            {
                var lambda = omc.TargetMember.IsEnumerable
                    ? EnumerableMappingLambdaFactory.Instance.Create<TSource, TTarget>(omc)
                    : ComplexTypeMappingLambdaFactory.Instance.Create<TSource, TTarget>(omc);

                return new ObjectMapper<TSource, TTarget>(lambda);
            });

            return mapper;
        }

        private class ObjectMapperKey
        {
            private readonly MappingRuleSet _ruleSet;
            private readonly string _sourceMemberSignature;
            private readonly string _targetMemberSignature;

            public ObjectMapperKey(IMemberMappingContext context)
            {
                _ruleSet = context.MappingContext.RuleSet;
                _sourceMemberSignature = context.SourceMember.Signature;
                _targetMemberSignature = context.TargetMember.Signature;
            }

            public override bool Equals(object obj)
            {
                var otherKey = (ObjectMapperKey)obj;

                return
                    (_ruleSet == otherKey._ruleSet) &&
                    (_sourceMemberSignature == otherKey._sourceMemberSignature) &&
                    (_targetMemberSignature == otherKey._targetMemberSignature);

            }

            public override int GetHashCode() => 0;
        }

        public void Reset()
        {
            _cache.Empty();
        }
    }
}