namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Caching;
    using Members;

    internal class ObjectMapperFactory
    {
        private readonly ICache<ObjectMapperKey, object> _cache;

        public ObjectMapperFactory()
        {
            _cache = GlobalContext.Instance.CreateCache<ObjectMapperKey, object>();
        }

        public IObjectMapper<TSource, TTarget> CreateFor<TSource, TTarget>(IObjectMapperCreationData data)
        {
            var mapper = (IObjectMapper<TSource, TTarget>)_cache.GetOrAdd(new ObjectMapperKey(data), k =>
            {
                var lambda = data.TargetMember.IsEnumerable
                    ? EnumerableMappingLambdaFactory.Instance.Create<TSource, TTarget>(data)
                    : ComplexTypeMappingLambdaFactory.Instance.Create<TSource, TTarget>(data);

                return new ObjectMapper<TSource, TTarget>(lambda);
            });

            return mapper;
        }

        public void Reset()
        {
            _cache.Empty();
        }
    }

    internal class ObjectMapperKey
    {
        private readonly MappingRuleSet _ruleSet;
        private readonly string _sourceMemberSignature;
        private readonly string _targetMemberSignature;

        public ObjectMapperKey(IObjectMapperCreationData data)
        {
            _ruleSet = data.RuleSet;
            _sourceMemberSignature = data.SourceMember.Signature;
            _targetMemberSignature = data.TargetMember.Signature;

            data.MapperData.SetKey(this);
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
}