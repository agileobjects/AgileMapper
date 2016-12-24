namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Extensions;

    internal static class DictionaryMemberMapperDataExtensions
    {
        public static Expression GetTargetMemberDictionaryKey(this IMemberMapperData mapperData)
        {
            var configuredKey = mapperData.MapperContext
                .UserConfigurations
                .Dictionaries
                .GetFullKeyOrNull(mapperData);

            if (configuredKey != null)
            {
                return configuredKey;
            }

            var keyParts = GetTargetMemberDictionaryKeyParts(mapperData);

            return keyParts.GetStringConcatCall();
        }

        public static IList<Expression> GetTargetMemberDictionaryKeyParts(this IMemberMapperData mapperData)
        {
            var joinedName = string.Empty;
            var memberPartExpressions = new List<Expression>();
            var joinedNameIsConstant = true;
            var parentCounterInaccessible = false;
            Expression parentContextAccess = null;

            while (!mapperData.IsRoot)
            {
                parentCounterInaccessible =
                    parentCounterInaccessible ||
                    mapperData.Context.IsStandalone ||
                    mapperData.TargetMember.IsRecursionRoot();

                if (mapperData.TargetMemberIsEnumerableElement())
                {
                    var index = GetEnumerableIndexAccess(parentContextAccess, mapperData);
                    AddEnumerableMemberNamePart(memberPartExpressions, mapperData, index);
                    joinedNameIsConstant = false;
                }
                else
                {
                    if (parentCounterInaccessible && (parentContextAccess == null))
                    {
                        parentContextAccess = mapperData.MappingDataObject;
                    }

                    var namePart = AddMemberNamePart(memberPartExpressions, mapperData);
                    joinedNameIsConstant = joinedNameIsConstant && namePart.NodeType == ExpressionType.Constant;

                    if (joinedNameIsConstant)
                    {
                        joinedName = (string)((ConstantExpression)namePart).Value + joinedName;
                    }
                }

                mapperData = mapperData.Parent;
            }

            if (joinedNameIsConstant)
            {
                memberPartExpressions.Clear();
                memberPartExpressions.Add(joinedName.ToConstantExpression());
            }

            return memberPartExpressions;
        }

        private static Expression GetEnumerableIndexAccess(Expression parentContextAccess, IMemberMapperData mapperData)
        {
            if (parentContextAccess == null)
            {
                return mapperData.Parent.EnumerablePopulationBuilder.Counter;
            }

            var mappingDataType = typeof(IMappingData<,>)
                .MakeGenericType(parentContextAccess.Type.GetGenericArguments());

            var enumerableIndexProperty = mappingDataType.GetProperty("EnumerableIndex");

            return Expression.Property(parentContextAccess, enumerableIndexProperty);
        }

        private static void AddEnumerableMemberNamePart(
            List<Expression> memberPartExpressions,
            IMemberMapperData mapperData,
            Expression index)
        {
            var elementKeyParts = GetTargetMemberDictionaryElementKeyParts(mapperData, index);

            memberPartExpressions.InsertRange(0, elementKeyParts);
        }

        public static IEnumerable<Expression> GetTargetMemberDictionaryElementKeyParts(
            this IMemberMapperData mapperData,
            Expression index)
        {
            return mapperData.MapperContext.UserConfigurations.Dictionaries.GetElementKeyParts(index, mapperData);
        }

        private static Expression AddMemberNamePart(
            IList<Expression> memberPartExpressions,
            IMemberMapperData mapperData)
        {
            var dictionarySettings = mapperData.MapperContext.UserConfigurations.Dictionaries;

            var memberName = dictionarySettings
                                 .GetMemberKeyOrNull(mapperData) ?? mapperData.TargetMember.LeafMember.JoiningName;

            var memberNamePart = dictionarySettings.GetJoiningName(memberName, mapperData);

            memberPartExpressions.Insert(0, memberNamePart);

            return memberNamePart;
        }
    }
}