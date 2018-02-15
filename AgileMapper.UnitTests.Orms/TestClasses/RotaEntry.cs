namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class RotaEntry
    {
        [Key]
        public int Id { get; set; }

        public int PersonId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public byte StartHour { get; set; }

        public byte StartMinute { get; set; }

        public byte EndHour { get; set; }

        public byte EndMinute { get; set; }
    }
}