namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class MergeEnumerablePopulationStrategy
    {
        public static Expression Create(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            if (builder.TargetElementsAreSimple)
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