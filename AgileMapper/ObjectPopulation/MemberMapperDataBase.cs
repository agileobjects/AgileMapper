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
        private Type _mappingDataType;
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

        #endregion

        public ObjectMapperData Parent { get; }

        public ParameterExpression MappingDataObject { get; }

        public Expression SourceObject
        {
            get => _sourceObject ??= GetSourceObject();
            set => _sourceObject = value;
        }

        protected virtual Expression GetSourceObject()
        {
            return IsEntryPoint
                ? SourceType.GetOrCreateSourceParameter()
                : GetNestedSourceObject();
        }

        protected virtual Expression GetNestedSourceObject()
            => SourceMember.GetQualifiedAccess(Parent.SourceObject);

        public Expression TargetObject
        {
            get => _targetObject ??= GetTargetObject();
            set => _targetObject = value;
        }

        private Expression GetTargetObject()
        {
            if (IsEntryPoint)
            {
                return TargetType.GetOrCreateTargetParameter();
            }

            return GetNestedTargetObject();

            //TargetObject = GetMappingDataProperty(Member.RootTargetMemberName);
        }

        protected virtual Expression GetNestedTargetObject()
            => TargetMember.GetQualifiedAccess(Parent.TargetObject);

        protected Expression GetParentObjectAccess()
            => GetMappingDataProperty(nameof(Parent));

        protected Expression GetMappingDataProperty(string propertyName)
        {
            var property = MappingDataType.GetPublicInstanceProperty(propertyName);

            return Expression.Property(MappingDataObject, property);
        }

        private Type MappingDataType
            => _mappingDataType ??= typeof(IMappingData<,>)
                .MakeGenericType(SourceType, TargetType);
    }
}