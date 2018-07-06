namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct MergeEnumerablePopulationStrategy : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            if (builder.ElementTypesAreSimple)
            {
                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType().ExcludingTargetItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(enumerableMappingData);
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.AddNewItemsToTargetVariable(enumerableMappingData);

            return builder;
        }
    }
}