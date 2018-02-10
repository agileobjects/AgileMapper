namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System.ComponentModel.DataAnnotations;

    public class Animal
    {
        [Key]
        public int Id { get; set; }

        public AnimalType Type { get; set; }

        public string Name { get; set; }

        public string Sound { get; set; }

        public enum AnimalType
        {
            Dog = 1,
            Elephant = 2,
            Snake = 3
        }
    }
}
