namespace AgileObjects.AgileMapper.Members
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
#if NET_STANDARD
    using System.Reflection;
#endif
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
            var joinedName = default(string);
            var memberPartExpressions = new List<Expression>();
            var joinedNameIsConstant = true;
            var parentCounterInaccessible = false;
            Expression parentContextAccess = null;

            foreach (var targetMember in mapperData.TargetMember.MemberChain.Reverse())
            {
                if (targetMember.IsRoot || IsRootDictionaryContext(mapperData))
                {
                    break;
                }

                parentCounterInaccessible =
                    parentCounterInaccessible ||
                    mapperData.Context.IsStandalone ||
                    mapperData.TargetMember.IsRecursionRoot();

                if (targetMember.IsEnumerableElement())
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

                    var namePart = AddMemberNamePart(memberPartExpressions, targetMember, mapperData);
                    joinedNameIsConstant = joinedNameIsConstant && namePart.NodeType == ExpressionType.Constant;

                    if (joinedNameIsConstant)
                    {
                        joinedName = (string)((ConstantExpression)namePart).Value + joinedName;
                    }
                }

                if (!mapperData.Parent.IsRoot)
                {
                    mapperData = mapperData.Parent;
                }
            }

            if (joinedNameIsConstant)
            {
                memberPartExpressions.Clear();
                memberPartExpressions.Add(joinedName.ToConstantExpression());
            }

            return memberPartExpressions;
        }

        private static bool IsRootDictionaryContext(IMemberMapperData mapperData)
        {
            if (!mapperData.SourceType.IsDictionary())
            {
                return false;
            }

            return !mapperData.Parent.SourceType.IsDictionary();
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
            Member targetMember,
            IMemberMapperData mapperData)
        {
            var memberNamePart = mapperData.MapperContext
                .UserConfigurations
                .Dictionaries
                .GetJoiningName(targetMember, mapperData);

            memberPartExpressions.Insert(0, memberNamePart);

            return memberNamePart;
        }
    }
}