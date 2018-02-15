namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;

    public class RotaEntryDto
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public int StartHour { get; set; }

        public int StartMinute { get; set; }

        public DateTime StartTime { get; set; }

        public int EndHour { get; set; }

        public int EndMinute { get; set; }

        public DateTime EndTime { get; set; }
    }
}