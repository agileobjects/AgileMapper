namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using System.Collections.Generic;

    internal class Order
    {
        public int OrderId { get; set; }

        public DateTime DateCreated { get; set; }

        public IEnumerable<OrderItem> Items { get; set; }
    }
}