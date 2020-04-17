namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal static class OverwriteEnumerablePopulationStrategy
    {
        public static Expression Create(
            EnumerablePopulationBuilder builder,
            IObjectMappingData enumerableMappingData)
        {
            if (builder.TargetElementsAreSimple)
            {
                if (builder.TargetTypeHelper.IsReadOnly)
                {
                    builder.PopulateTargetVariableFromSourceObjectOnly();
                    return builder;
                }

                builder.AssignSourceVariableTo(s => s.SourceItemsProjectedToTargetType());
                builder.AssignTargetVariable();
                builder.RemoveAllTargetItems();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            if (builder.ElementsAreIdentifiable)
            {
                builder.CreateCollectionData();
                builder.MapIntersection(enumerableMappingData);
                builder.AssignSourceVariableTo(s => s.CollectionDataNewSourceItems());
                builder.AssignTargetVariable();
                builder.RemoveTargetItemsById();
                builder.AddNewItemsToTargetVariable(enumerableMappingData);

                return builder;
            }

            builder.AssignSourceVariableToSourceObject();
            builder.AssignTargetVariable();
            builder.RemoveAllTargetItems();
            builder.AddNewItemsToTargetVariable(enumerableMappingData);

            return builder;
        }
    }
}
