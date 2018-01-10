namespace AgileObjects.AgileMapper.UnitTests.TestClasses
{
    using System;
    using System.Collections.Generic;

    internal class OrderEntity : EntityBase
    {
        public DateTime DateCreated { get; set; }

        public ICollection<OrderItemEntity> Items { get; set; }
    }
}