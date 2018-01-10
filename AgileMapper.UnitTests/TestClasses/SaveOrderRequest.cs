namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using System.Collections.Generic;

    internal class SaveOrderRequest : DtoBase
    {
        public DateTime DateCreated { get; set; }

        public ICollection<SaveOrderItemRequest> Items { get; set; }
    }
}