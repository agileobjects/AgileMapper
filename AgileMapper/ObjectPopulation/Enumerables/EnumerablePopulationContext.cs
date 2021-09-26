namespace AgileObjects.AgileMapper.ObjectPopulation.Enumerables
{
    using System;
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif
    using Extensions;
    using Extensions.Internal;
    using Members;
    using NetStandardPolyfills;
    using ReadableExpressions.Extensions;
    using static Members.Member;

    internal class EnumerablePopulationContext
    {
        private Type _mappingSourceElementType;

        public EnumerablePopulationContext(IQualifiedMemberContext context)
        {
            _mappingSourceElementType = SourceElementType = context.SourceMember.ElementType;
            TargetTypeHelper = new EnumerableTypeHelper(context.TargetMember);

            if (SourceElementType == null)
            {
                return;
            }

            TargetElementType = context.TargetMember.GetElementType(SourceElementType);
            ElementTypes = new[] { SourceElementType, TargetElementType };
            ElementTypesAreTheSame = SourceElementType == TargetElementType;
            TargetElementsAreSimple = TargetElementType.IsSimple();
        }

        public Type SourceElementType { get; }

        public Type TargetElementType { get; }

        public Type[] ElementTypes { get; }

        /// <summary>
        /// Gets a value indicating whether the mapping is being performed between collections with
        /// the same element Types.
        /// </summary>
        public bool ElementTypesAreTheSame { get; }

        public bool ElementTypesAreAssignable => SourceElementType.IsAssignableTo(TargetElementType);

        public bool TargetElementsAreSimple { get; }

        public EnumerableTypeHelper SourceTypeHelper { get; private set; }

        public void CreateSourceTypeHelper(Expression sourceValue)
        {
            _mappingSourceElementType = ElementTypesAreTheSame
                ? TargetElementType
                : sourceValue.Type.GetEnumerableElementType();

            SourceTypeHelper = new EnumerableTypeHelper(sourceValue.Type, _mappingSourceElementType);
        }

        public EnumerableTypeHelper TargetTypeHelper { get; }

        public ParameterExpression GetSourceParameterFor(Type type) 
            => GetParameterFor(type, prefix: null, RootSourceMemberName, suffix: null);

        public ParameterExpression GetSourceElementParameterFor(Type type, string prefix = null)
            => GetParameterFor(type, prefix, RootSourceMemberName, suffix: "Element");

        public ParameterExpression GetTargetParameterFor(Type type)
            => GetParameterFor(type, prefix: null, RootTargetMemberName, suffix: null);

        public ParameterExpression GetTargetElementParameterFor(Type type, string prefix = null)
            => GetParameterFor(type, prefix, RootTargetMemberName, suffix: "Element");

        private ParameterExpression GetParameterFor(
            Type type,
            string prefix,
            string sameTypesPrefix,
            string suffix)
        {
            string parameterName;

            if (_mappingSourceElementType == TargetElementType)
            {
                parameterName = prefix != null
                    ? prefix + sameTypesPrefix.ToPascalCase()
                    : sameTypesPrefix;

                parameterName += type.GetVariableNameInPascalCase();
            }
            else
            {
                parameterName = prefix != null
                    ? prefix + type.GetVariableNameInPascalCase()
                    : type.GetVariableNameInCamelCase();
            }

            if (suffix != null)
            {
                parameterName += suffix;
            }

            return type.GetOrCreateParameter(parameterName);
        }
    }
}