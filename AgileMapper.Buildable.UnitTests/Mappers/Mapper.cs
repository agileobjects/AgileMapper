namespace AgileObjects.AgileMapper.Buildable.UnitTests.Mappers
{
    public static class Mapper
    {
        public static StringMapper Map
        (
            string source
        )
        {
            return new StringMapper(source);
        }
    }
}