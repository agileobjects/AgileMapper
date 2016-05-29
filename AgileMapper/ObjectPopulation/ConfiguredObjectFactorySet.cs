namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Members;

    internal class ConfiguredObjectFactorySet
    {
        private readonly Dictionary<Type, LambdaExpression> _factoriesByObjectType;

        public ConfiguredObjectFactorySet()
        {
            _factoriesByObjectType = new Dictionary<Type, LambdaExpression>();
        }

        public void Add(Type objectType, LambdaExpression objectFactory)
        {
            _factoriesByObjectType.Add(objectType, objectFactory);
        }

        public Expression GetFactoryOrNull(IMemberMappingContext context)
        {
            var objectType = context.InstanceVariable.Type;

            LambdaExpression objectFactory;

            if (_factoriesByObjectType.TryGetValue(objectType, out objectFactory))
            {
                return CreateFrom(objectFactory);
            }

            var matchingKey = _factoriesByObjectType.Keys
                .FirstOrDefault(idType => idType.IsAssignableFrom(objectType));

            return (matchingKey != null) ? CreateFrom(_factoriesByObjectType[matchingKey]) : null;
        }

        private static Expression CreateFrom(LambdaExpression objectFactory)
        {
            return objectFactory.Body;
        }
    }
}