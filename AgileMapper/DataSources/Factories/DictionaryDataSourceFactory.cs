namespace AgileObjects.AgileMapper.DataSources.Factories
{
    using System;
    using System.Collections.Generic;
    using Extensions.Internal;
    using Members;
    using Members.Dictionaries;
    using ReadableExpressions.Extensions;

    internal static class DictionaryDataSourceFactory
    {
        public static bool TryGet(
            IMemberMapperData mapperData,
            out MaptimeDataSourceFactory maptimeDataSourceFactory)
        {
            if ((mapperData.TargetMember.IsComplex && mapperData.Context.IsStandalone) ||
                 DoesNotHaveUseableSourceDictionary(mapperData))
            {
                maptimeDataSourceFactory = null;
                return false;
            }

            maptimeDataSourceFactory = Create;
            return true;
        }

        private static bool DoesNotHaveUseableSourceDictionary(IMemberMapperData mapperData)
        {
            if (!mapperData.SourceMemberIsStringKeyedDictionary(out var dictionarySourceMember))
            {
                return true;
            }

            if (dictionarySourceMember.HasObjectEntries)
            {
                return false;
            }

            var valueType = dictionarySourceMember.ValueType;

            Type targetType;

            if (mapperData.TargetMember.IsEnumerable)
            {
                if (valueType.IsEnumerable())
                {
                    return false;
                }

                targetType = mapperData.TargetMember.ElementType;

                if ((valueType == targetType) || targetType.IsComplex())
                {
                    return false;
                }
            }
            else
            {
                targetType = mapperData.TargetMember.Type;
            }

            return !mapperData.CanConvert(valueType, targetType);
        }

        private static IEnumerable<IDataSource> Create(IChildMemberMappingData mappingData)
        {
            var mapperData = mappingData.MapperData;

            if (mapperData.TargetMember.IsSimple)
            {
                yield return new DictionaryEntryDataSource(new DictionaryEntryVariablePair(mapperData));
                yield break;
            }

            var sourceMember = GetSourceMember(mapperData);

            yield return new DictionaryNonSimpleMemberDataSource(sourceMember, mapperData);
        }

        private static DictionarySourceMember GetSourceMember(IMemberMapperData mapperData)
        {
            if (!mapperData.TargetMember.IsRecursion)
            {
                return new DictionarySourceMember(mapperData);
            }

            var parentMapperData = mapperData.Parent;

            while (!parentMapperData.IsRoot)
            {
                if (parentMapperData.TargetMember.LeafMember.Equals(mapperData.TargetMember.LeafMember))
                {
                    break;
                }

                parentMapperData = parentMapperData.Parent;
            }

            return (DictionarySourceMember)parentMapperData.SourceMember;
        }
    }
}