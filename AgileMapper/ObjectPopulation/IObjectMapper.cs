namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System.Collections.Generic;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using MapperKeys;
    using RepeatedMappings;

    internal interface IObjectMapper : IObjectMapperFunc
    {
        ObjectMapperData MapperData { get; }

        LambdaExpression GetMappingLambda();

        IEnumerable<IRepeatedMapperFunc> RepeatedMappingFuncs { get; }

        void CacheRepeatedMappingFuncs();

        bool IsStaticallyCacheable();

        object MapSubObject(
            object source,
            object target,
            IMappingExecutionContext context,
            ObjectMapperKeyBase mapperKey);

        object MapRepeated(
            object source,
            object target,
            IMappingExecutionContext context,
            ObjectMapperKeyBase mapperKey);

        void Reset();
    }
}