namespace AgileObjects.AgileMapper.PerformanceTester.ConcreteMappers.Manual
{
    using AbstractMappers;
    using static TestClasses.Ctor;

    internal class ManualCtorMapper : CtorMapperBase
    {
        public override void Initialise()
        {
        }

        protected override ConstructedObject Construct(ValueObject valueObject)
        {
            if (valueObject == null)
            {
                return null;
            }

            return new ConstructedObject(valueObject.Value);
        }
    }
}