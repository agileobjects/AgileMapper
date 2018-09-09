namespace AgileObjects.AgileMapper.PerformanceTesting.ConcreteMappers.Manual
{
    using AbstractMappers;
    using static TestClasses.Ctor;

    public class ManualCtorMapper : CtorMapperBase
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