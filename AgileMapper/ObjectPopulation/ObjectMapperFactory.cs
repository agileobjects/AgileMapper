namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using Members;

    internal class ObjectMapperFactory
    {
        public IObjectMapper<TTarget> CreateFor<TSource, TTarget>(IObjectMappingContext omc)
        {
            var mapper = omc.MapperContext.Cache.GetOrAdd(new ObjectMapperKey(omc), k =>
            {
                var lambda = omc.TargetMember.IsEnumerable
                    ? EnumerableMappingLambdaFactory<TSource, TTarget>.Instance.Create(omc)
                    : ComplexTypeMappingLambdaFactory<TSource, TTarget>.Instance.Create(omc);

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
                var otherKey = obj as ObjectMapperKey;

                if (otherKey == null)
                {
                    return false;
                }

                return
                    (_ruleSet == otherKey._ruleSet) &&
                    (_sourceMemberSignature == otherKey._sourceMemberSignature) &&
                    (_targetMemberSignature == otherKey._targetMemberSignature);

            }

            public override int GetHashCode() => 0;
        }
    }
}