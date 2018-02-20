namespace AgileObjects.AgileMapper.UnitTests.Orms.TestClasses
{
    using System;
    using System.Collections.Generic;

    public class OrderDto
    {
        public int Id { get; set; }

        public DateTime DatePlaced { get; set; }

        public int HasItems { get; set; }

        public IEnumerable<OrderItemDto> Items { get; set; }
    }
}