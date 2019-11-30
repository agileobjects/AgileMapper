namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Globalization;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;

    internal abstract class MemberMapperDataBase : BasicMapperData
    {
        protected MemberMapperDataBase(
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            MapperContext mapperContext,
            ObjectMapperData parent)
            : base(
                ruleSet,
                sourceMember.Type,
                targetMember.Type,
                sourceMember,
                targetMember,
                parent)
        {
            MapperContext = mapperContext;
            Parent = parent;
            MappingDataObject = CreateMappingDataObject();
            MappingDataType = typeof(IMappingData<,>).MakeGenericType(SourceType, TargetType);
            SourceObject = GetMappingDataProperty(MappingDataType, Member.RootSourceMemberName);
            TargetObject = GetMappingDataProperty(Member.RootTargetMemberName);
        }

        public MapperContext MapperContext { get; }

        public ObjectMapperData Parent { get; }

        public ParameterExpression MappingDataObject { get; }
        
        public Expression SourceObject { get; set; }
        
        public Expression TargetObject { get; set; }

        protected ParameterExpression CreateMappingDataObject()
        {
            var mdType = typeof(IObjectMappingData<,>).MakeGenericType(SourceType, TargetType);

            var parent = Parent;
            var variableNameIndex = default(int?);

            while (parent != null)
            {
                if (parent.MappingDataObject.Type == mdType)
                {
                    variableNameIndex = variableNameIndex.HasValue ? (variableNameIndex + 1) : 2;
                }

                parent = parent.Parent;
            }

            var mappingDataVariableName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}To{1}Data{2}",
                SourceType.GetShortVariableName(),
                TargetType.GetShortVariableName().ToPascalCase(),
                variableNameIndex);

            return Expression.Parameter(mdType, mappingDataVariableName);
        }

        protected Type MappingDataType { get; }

        protected Expression GetElementIndexAccess()
            => GetMappingDataProperty(MappingDataType, "ElementIndex");

        protected Expression GetParentObjectAccess()
            => GetMappingDataProperty(nameof(Parent));

        protected Expression GetMappingDataProperty(Type mappingDataType, string propertyName)
        {
            var property = mappingDataType.GetPublicInstanceProperty(propertyName);

            return Expression.Property(MappingDataObject, property);
        }

        protected Expression GetMappingDataProperty(string propertyName)
            => Expression.Property(MappingDataObject, propertyName);
    }
}