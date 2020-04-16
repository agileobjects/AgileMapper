namespace AgileObjects.AgileMapper.Configuration.Lambdas
{
#if NET35
    using Microsoft.Scripting.Ast;
#else
    using System.Linq.Expressions;
#endif

    internal class RequiredValuesSet
    {
        private Expression _mappingContext;
        private Expression _parent;
        private Expression _source;
        private Expression _target;
        private Expression _createdObject;
        private Expression _elementIndex;
        private Expression _elementKey;
        private LambdaValue _values;

        public Expression MappingContext
        {
            get => _mappingContext;
            set
            {
                if (AppendIfMissing(LambdaValue.MappingContext))
                {
                    _mappingContext = value;
                }
            }
        }

        public Expression Parent
        {
            get => _parent;
            set
            {
                if (AppendIfMissing(LambdaValue.Parent))
                {
                    _parent = value;
                }
            }
        }

        public Expression Source
        {
            get => _source;
            set
            {
                if (AppendIfMissing(LambdaValue.Source))
                {
                    _source = value;
                }
            }
        }

        public Expression Target
        {
            get => _target;
            set
            {
                if (AppendIfMissing(LambdaValue.Target))
                {
                    _target = value;
                }
            }
        }

        public Expression CreatedObject
        {
            get => _createdObject;
            set
            {
                if (AppendIfMissing(LambdaValue.CreatedObject))
                {
                    _createdObject = value;
                }
            }
        }

        public Expression ElementIndex
        {
            get => _elementIndex;
            set
            {
                if (AppendIfMissing(LambdaValue.ElementIndex))
                {
                    _elementIndex = value;
                }
            }
        }

        public Expression ElementKey
        {
            get => _elementKey;
            set
            {
                if (AppendIfMissing(LambdaValue.ElementKey))
                {
                    _elementKey = value;
                }
            }
        }

        public int ValuesCount { get; private set; }

        private bool AppendIfMissing(LambdaValue requiredValue)
        {
            if (Includes(requiredValue))
            {
                return false;
            }

            _values |= requiredValue;
            ++ValuesCount;
            return true;
        }

        public bool Includes(LambdaValue value) => _values.Has(value);
    }
}