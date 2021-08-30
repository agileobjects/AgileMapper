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

    internal abstract class MemberMapperDataBase : QualifiedMemberContext
    {
        private Expression _sourceObject;
        private Expression _targetObject;

        protected MemberMapperDataBase(
            MappingRuleSet ruleSet,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData parent,
            MapperContext mapperContext)
            : base(
                ruleSet,
                sourceMember.Type,
                targetMember.Type,
                sourceMember,
                targetMember,
                parent,
                mapperContext)
        {
            Parent = parent;
            MappingDataObject = CreateMappingDataObject();
            MappingDataType = typeof(IMappingData<,>).MakeGenericType(SourceType, TargetType);
        }

        #region Setup

        private ParameterExpression CreateMappingDataObject()
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

        private Expression GetMappingDataProperty(Type mappingDataType, string propertyName)
        {
            var property = mappingDataType.GetPublicInstanceProperty(propertyName);

            return Expression.Property(MappingDataObject, property);
        }

        protected Expression GetMappingDataProperty(string propertyName)
            => Expression.Property(MappingDataObject, propertyName);

        #endregion

        public ObjectMapperData Parent { get; }

        public ParameterExpression MappingDataObject { get; }

        public Expression SourceObject
        {
            get
            {
                if (_sourceObject == null)
                {
                    PopulateSourceAndTarget();
                }

                return _sourceObject;
            }
            set => _sourceObject = value;
        }

        public Expression TargetObject
        {
            get
            {
                if (_targetObject == null)
                {
                    PopulateSourceAndTarget();
                }

                return _targetObject;
            }
            set => _targetObject = value;
        }

        private void PopulateSourceAndTarget()
        {
            if (IsEntryPoint)
            {
                SourceObject = SourceType.GetOrCreateSourceParameter();
                TargetObject = TargetType.GetOrCreateTargetParameter();
                return;
            }

            SourceObject = SourceMember.GetQualifiedAccess(Parent.SourceObject);
            TargetObject = TargetMember.GetQualifiedAccess(Parent.TargetObject);

            //SourceObject = GetMappingDataProperty(MappingDataType, Member.RootSourceMemberName);
            //TargetObject = GetMappingDataProperty(Member.RootTargetMemberName);
        }

        private Type MappingDataType { get; }

        protected Expression GetElementIndexAccess()
            => GetMappingDataProperty(MappingDataType, nameof(IMemberMapperData.ElementIndex));

        protected Expression GetElementKeyAccess()
            => GetMappingDataProperty(MappingDataType, nameof(IMemberMapperData.ElementKey));

        protected Expression GetParentObjectAccess()
            => GetMappingDataProperty(nameof(Parent));
    }
}