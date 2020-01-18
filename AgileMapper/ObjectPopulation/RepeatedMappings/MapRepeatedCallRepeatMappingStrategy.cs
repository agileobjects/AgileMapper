namespace AgileObjects.AgileMapper.ObjectPopulation.RepeatedMappings
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Members;
    using ReadableExpressions.Extensions;

    internal struct MapRepeatedCallRepeatMappingStrategy : IRepeatMappingStrategy
    {
        public bool AppliesTo(IQualifiedMemberContext context) => !context.TargetMember.IsEnumerable;

        public bool WillNotMap(IQualifiedMemberContext context)
        {
            if (!context.TargetMember.IsRecursion)
            {
                return false;
            }

            if (context.SourceType.IsDictionary())
            {
                return context.TargetMember.Depth > 3;
            }

            while (context != null)
            {
                if (context.TargetMember.IsDictionary ||
                    context.TargetType.Name.EndsWith("Dto", StringComparison.Ordinal) ||
                    context.TargetType.Name.EndsWith("DataTransferObject", StringComparison.Ordinal))
                {
                    return true;
                }

                context = context.Parent;
            }

            return false;
        }

        public Expression GetMapRepeatedCallFor(
            IObjectMappingData mappingData,
            MappingValues mappingValues,
            int dataSourceIndex,
            ObjectMapperData declaredTypeMapperData)
        {
            var childMapperData = mappingData.MapperData;

            if (WillNotMap(childMapperData))
            {
                return Constants.EmptyExpression;
            }

            childMapperData.CacheMappedObjects = true;

            childMapperData.RegisterRepeatedMapperFunc(mappingData);

            var mapRepeatedCall = declaredTypeMapperData.GetMapRepeatedCall(
                childMapperData.TargetMember,
                mappingValues,
                dataSourceIndex);

            return mapRepeatedCall;
        }
    }
}