namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using System.Collections.Generic;

    internal class OrderDto : DtoBase
    {
        public DateTime DateCreated { get; set; }

        public ICollection<OrderItemDto> Items { get; set; }
    }
}