namespace AgileObjects.AgileMapper.ObjectPopulation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        private IObjectMappingData _mappingData;

        public ObjectMapperData(
            IObjectMappingData mappingData,
            IQualifiedMember sourceMember,
            QualifiedMember targetMember,
            int? dataSourceIndex,
            ObjectMapperData parent,
            bool isForDerivedTypeMappingRoot = false)
            : base(
                  mappingData.RuleSet,
                  sourceMember.Type,
                  targetMember.Type,
                  targetMember,
                  parent)
        {
            MapperContext = mappingData.MappingContext.MapperContext;
            MappingData = mappingData;
            _childMapperDatas = new List<ObjectMapperData>();
            DataSourceIndex = dataSourceIndex.GetValueOrDefault();

            MappingDataObject = GetMappingDataObject();
            SourceMember = sourceMember;
            ParentObject = Expression.Property(MappingDataObject, "Parent");
            SourceObject = Expression.Property(MappingDataObject, "Source");
            TargetObject = Expression.Property(MappingDataObject, "Target");
            CreatedObject = Expression.Property(MappingDataObject, "CreatedObject");
            EnumerableIndex = Expression.Property(MappingDataObject, "EnumerableIndex");
            NestedAccessFinder = new NestedAccessFinder(MappingDataObject);

            _mapObjectMethod = GetMapMethod(MappingDataObject.Type, 4);
            _mapEnumerableElementMethod = GetMapMethod(MappingDataObject.Type, 3);

            _dataSourcesByTargetMemberName = new Dictionary<string, DataSourceSet>();

            if (targetMember.IsEnumerable)
            {
                RequiresElementMapping = !targetMember.ElementType.IsSimple();
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

            IsForDerivedTypeMappingRoot = isForDerivedTypeMappingRoot;
            ReturnLabelTarget = Expression.Label(TargetType, "Return");

            if (IsRoot)
            {
                return;
            }

            parent._childMapperDatas.Add(this);
            Parent = parent;
        }

        #region Setup

        private ParameterExpression GetMappingDataObject()
        {
            var mdType = typeof(ObjectMappingData<,>).MakeGenericType(SourceType, TargetType);

            var mappingDataVariableName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}To{1}Data",
                SourceType.GetShortVariableName(),
                TargetType.GetShortVariableName().ToPascalCase());

            return Parameters.Create(mdType, mappingDataVariableName);
        }

        private bool IsTargetTypeFirstMapping()
        {
            if (IsRoot)
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

        private bool IsTargetTypeLastMapping() => !TypeHasACompatibleChildMember(TargetType, TargetType);

        private static bool TypeHasACompatibleChildMember(Type targetType, Type parentType)
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

                    return TypeHasACompatibleChildMember(targetType, childMember.Type);
                }

                if (childMember.ElementType.IsComplex() && childMember.ElementType.IsAssignableFrom(targetType))
                {
                    return true;
                }

                return TypeHasACompatibleChildMember(targetType, childMember.ElementType);
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

        public bool IsForDerivedTypeMappingRoot { get; }

        public int DataSourceIndex { get; set; }

        public IObjectMappingData MappingData
        {
            get
            {
                if (_mappingData != null)
                {
                    return _mappingData;
                }

                // A parent MapperData which maps child or element members using 
                // a MappingData.Map() call will have a null MappingData when the
                // Mapper is created for the child or element. In that circumstance
                // it can be retrieved via the child MapperData:
                return _childMapperDatas
                    .Select(cmd => cmd.MappingData?.Parent)
                    .FirstOrDefault(md => md != null);
            }
            set { _mappingData = value; }
        }

        public bool RequiresChildMapping => _dataSourcesByTargetMemberName.Any();

        public bool RequiresElementMapping { get; }

        public IQualifiedMember GetSourceMemberFor(string targetMemberRegistrationName, int dataSourceIndex)
            => _dataSourcesByTargetMemberName[targetMemberRegistrationName][dataSourceIndex].SourceMember;

        public QualifiedMember GetTargetMemberFor(string targetMemberRegistrationName)
            => TargetMember.GetChildMember(targetMemberRegistrationName);

        public ParameterExpression MappingDataObject { get; }

        public Expression ParentObject { get; }

        public IQualifiedMember SourceMember { get; }

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
                MappingDataObject,
                _mapObjectMethod.MakeGenericMethod(sourceObject.Type, targetMember.Type),
                sourceObject,
                targetMember.GetAccess(InstanceVariable),
                Expression.Constant(targetMember.RegistrationName),
                Expression.Constant(dataSourceIndex));

            return mapCall;
        }

        public MethodCallExpression GetMapCall(Expression sourceElement, Expression targetElement)
        {
            var mapCall = Expression.Call(
                MappingDataObject,
                _mapEnumerableElementMethod.MakeGenericMethod(sourceElement.Type, targetElement.Type),
                sourceElement,
                targetElement,
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

        public ObjectMapperData WithTypes(Type sourceType, Type targetType)
        {
            var typedSourceMember = SourceMember.WithType(sourceType);
            var typedTargetMember = TargetMember.WithType(targetType);

            return new ObjectMapperData(
                MappingData,
                typedSourceMember,
                typedTargetMember,
                DataSourceIndex,
                Parent,
                isForDerivedTypeMappingRoot: true);
        }
    }
}