namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal struct OverwriteEnumerablePopulationStrategy : IEnumerablePopulationStrategy
    {
        public Expression GetPopulation(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            if (builder.ElementTypesAreSimple)
            {
                if (builder.TargetTypeHelper.IsReadOnly)
                {
                    builder.PopulateTargetVariableFromSourceObjectOnly();
                    return builder;
                }

                builder.AssignSourceVariableFrom(s => s.SourceItemsProjectedToTargetType());
                builder.AssignTargetVariable();
                builder.RemoveAllTargetItems();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(enumerableMappingData);
                builder.AssignSourceVariableFrom(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.RemoveTargetItemsById();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            builder.AssignSourceVariableFromSourceObject();
            builder.AssignTargetVariable();
            builder.RemoveAllTargetItems();
            builder.AddNewItemsToTargetVariable(enumerableMappingData);

            return builder;
        }
    }
}
