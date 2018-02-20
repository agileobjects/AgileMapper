namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.Collections.Generic;

    public class RotaDto
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool HasEntries { get; set; }

        public ICollection<RotaEntryDto> Entries { get; set; }
    }
}