namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using DataSources;
    using Extensions;
    using Members;

    internal class ObjectMapperData : BasicMapperData, IMemberMapperData
    {
        private readonly List<ObjectMapperData> _childMapperDatas;
        private readonly MethodInfo _mapObjectMethod;
        private readonly MethodInfo _mapEnumerableElementMethod;
        private readonly Dictionary<string, DataSourceSet> _dataSourcesByTargetMemberName;
        private readonly List<string> _inlineMappingTargetMemberNames;
        private bool _elementMappingInlined;

        public ObjectMapperData(
            IObjectMappingData mappingData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            ObjectMapperData parent)
            : base(
                  mappingData.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            MappingData = mappingData;
            Parent = parent;
            Parent?._childMapperDatas.Add(this);
            _childMapperDatas = new List<ObjectMapperData>();

            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(sourceMember.Type, targetMember.Type);
            Parameter = Parameters.Create(mdType, "data");
            SourceMember = sourceMember;
            ParentObject = Expression.Property(Parameter, "Parent");
            SourceObject = Expression.Property(Parameter, "Source");
            TargetObject = Expression.Property(Parameter, "Target");
            CreatedObject = Expression.Property(Parameter, "CreatedObject");
            EnumerableIndex = Expression.Property(Parameter, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(Parameter);

            _mapObjectMethod = GetMapMethod(mdType, 4);
            _mapEnumerableElementMethod = GetMapMethod(mdType, 3);

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();
            _inlineMappingTargetMemberNames = new List<string>();

            if (targetMember.IsEnumerable)
            {
                if (!targetMember.ElementType.IsSimple())
                {
                    SourceElementMember = sourceMember.Append(sourceMember.Type.CreateElementMember());
                    TargetElementMember = targetMember.Append(targetMember.Type.CreateElementMember(targetMember.ElementType));
                }

                EnumerablePopulationBuilder = new EnumerablePopulationBuilder(this);
                InstanceVariable = EnumerablePopulationBuilder.TargetVariable;
            }
            else
            {
                TargetTypeHasNotYetBeenMapped = IsTargetTypeFirstMapping();
                TargetTypeWillNotBeMappedAgain = IsTargetTypeLastMapping();

                InstanceVariable = Expression
                    .Variable(TargetType, TargetType.GetVariableNameInCamelCase());
            }

            ReturnLabelTarget = Expression.Label(TargetType, "Return");
        }

        #region Setup

        private bool IsTargetTypeFirstMapping()
        {
            if (Parent == null)
            {
                return true;
            }

            var parent = Parent;

            while (parent != null)
            {
                if (parent.HasTypeBeenMapped(TargetType, this))
                {
                    return false;
                }

                parent = parent.Parent;
            }

            return true;
        }

        private bool IsTargetTypeLastMapping() => !DoesTypeHaveACompatibleChildMember(TargetType, TargetType);

        private static bool DoesTypeHaveACompatibleChildMember(Type targetType, Type parentType)
        {
            var childTargetMembers = GlobalContext.Instance.MemberFinder.GetWriteableMembers(parentType);

            foreach (var childMember in childTargetMembers)
            {
                if (childMember.IsSimple)
                {
                    continue;
                }

                if (childMember.IsComplex)
                {
                    if (childMember.Type.IsAssignableFrom(targetType))
                    {
                        return true;
                    }

                    return DoesTypeHaveACompatibleChildMember(targetType, childMember.Type);
                }

                if (childMember.ElementType.IsComplex() && childMember.ElementType.IsAssignableFrom(targetType))
                {
                    return true;
                }

                return DoesTypeHaveACompatibleChildMember(targetType, childMember.ElementType);
            }

            return false;
        }

        private static MethodInfo GetMapMethod(Type mappingDataType, int numberOfArguments)
        {
            return mappingDataType
                .GetPublicInstanceMethods()
                .First(m => (m.Name == "Map") && (m.GetParameters().Length == numberOfArguments));
        }

        #endregion

        public MapperContext MapperContext { get; }

        public ObjectMapperData Parent { get; }

        public IObjectMappingData MappingData { get; set; }

        public bool RequiresChildMapping
            => _dataSourcesByTargetMemberName.Any(kvp => _inlineMappingTargetMemberNames.DoesNotContain(kvp.Key));

        public bool RequiresElementMapping => (TargetElementMember != null) && !_elementMappingInlined;

        private bool HasTypeBeenMapped(Type targetType, IBasicMapperData requestingMapperData)
        {
            var mappedType = TargetMember.IsEnumerable ? TargetMember.ElementType : TargetType;

            if (mappedType.IsAssignableFrom(targetType))
            {
                return true;
            }

            foreach (var childMapperData in _childMapperDatas)
            {
                if (childMapperData == requestingMapperData)
                {
                    break;
                }

                if (childMapperData.HasTypeBeenMapped(targetType, this))
                {
                    return true;
                }
            }

            if ((Parent != null) && (requestingMapperData != Parent))
            {
                return Parent.HasTypeBeenMapped(targetType, this);
            }

            return false;
        }

        public ObjectMapperData GetChildMapperDataFor(string targetMemberRegistrationName, int dataSourceIndex)
        {
            var sourceMember = GetSourceMemberFor(targetMemberRegistrationName, dataSourceIndex);
            var targetMember = GetTargetMemberFor(targetMemberRegistrationName);

            return GetChildMapperDataFor(sourceMember, targetMember);
        }

        public ObjectMapperData GetElementMapperData()
            => GetChildMapperDataFor(SourceElementMember, TargetElementMember);

        private ObjectMapperData GetChildMapperDataFor(IQualifiedMember sourceMember, QualifiedMember targetMember)
        {
            return _childMapperDatas
                .First(md => (md.SourceMember == sourceMember) && (md.TargetMember == targetMember));
        }

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberRegistrationName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberRegistrationName)
            => TargetMember.GetChildMember(targetMemberRegistrationName);

        public ParameterExpression Parameter { get; }

        public Expression ParentObject { get; }

        public IQualifiedMember SourceMember { get; }

        public IQualifiedMember SourceElementMember { get; }

        public QualifiedMember TargetElementMember { get; }

        public bool TargetTypeHasNotYetBeenMapped { get; }

        public bool TargetTypeWillNotBeMappedAgain { get; }

        public Expression SourceObject { get; }

        public Expression TargetObject { get; }

        public Expression EnumerableIndex { get; }

        public ParameterExpression InstanceVariable { get; }

        public NestedAccessFinder NestedAccessFinder { get; }

        public EnumerablePopulationBuilder EnumerablePopulationBuilder { get; }

        public Expression CreatedObject { get; }

        public LabelTarget ReturnLabelTarget { get; }

        public MethodCallExpression GetMapCall(
            Expression sourceObject,
            QualifiedMember targetMember,
            int dataSourceIndex)
        {
            var mapCall = Expression.Call(
                Parameter,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(InstanceVariable),
                Expression.Constant(targetMember.RegistrationName),
                Expression.Constant(dataSourceIndex));

            return mapCall;
        }

        public MethodCallExpression GetMapCall(Expression sourceElement, Expression existingElement)
        {
            var mapCall = Expression.Call(
                Parameter,
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, existingElement.Type),
                sourceElement,
                existingElement,
                Parameters.EnumerableIndex);

            return mapCall;
        }

        public void RegisterTargetMemberDataSourcesIfRequired(QualifiedMember targetMember, DataSourceSet dataSources)
        {
            if (targetMember.IsSimple)
            {
                return;
            }

            _dataSourcesByTargetMemberName.Add(targetMember.RegistrationName, dataSources);
        }

        public void ElementMappingInlined() => _elementMappingInlined = true;

        public void MappingInlinedFor(QualifiedMember targetMember)
            => _inlineMappingTargetMemberNames.Add(targetMember.RegistrationName);
    }
}