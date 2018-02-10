namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public abstract class AnimalDtoBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public abstract string Sound { get; }
    }
}