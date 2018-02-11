namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    public class DogDto : AnimalDtoBase
    {
        public override string Sound
        {
            get => "Woof";
            set { }
        }
    }
}