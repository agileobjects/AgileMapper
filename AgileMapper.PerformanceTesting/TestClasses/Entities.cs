namespace AgileObjects.AgileMapper.PerformanceTesting.TestClasses
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public static class Entities
    {
        public abstract class EntityBase
        {
            [Key]
            public int Id { get; set; }
        }

        public class Tag : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public HashSet<BranchTag> Branches { get; set; } = new HashSet<BranchTag>();

            public HashSet<LocationTag> Locations { get; set; } = new HashSet<LocationTag>();

            public HashSet<WarehouseTag> Warehouses { get; set; } = new HashSet<WarehouseTag>();

            public HashSet<ProductTag> Products { get; set; } = new HashSet<ProductTag>();

            public HashSet<MovementTag> Movements { get; set; } = new HashSet<MovementTag>();

            public HashSet<AssociateTag> Associates { get; set; } = new HashSet<AssociateTag>();

            public HashSet<WarehouseProductTag> WarehouseProducts { get; set; } = new HashSet<WarehouseProductTag>();
        }

        public class BranchTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int BranchId { get; set; }

            public Branch Branch { get; set; }
        }

        public class Branch : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public Location Location { get; set; }

            public HashSet<Warehouse> Warehouses { get; set; } = new HashSet<Warehouse>();

            public HashSet<BranchTag> Tags { get; set; } = new HashSet<BranchTag>();
        }

        public class LocationTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int LocationId { get; set; }

            public Location Location { get; set; }
        }

        public class Location : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public HashSet<LocationTag> Tags { get; set; } = new HashSet<LocationTag>();
        }

        public class WarehouseTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int WarehouseId { get; set; }

            public Warehouse Warehouse { get; set; }
        }

        public class Warehouse : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public int BranchId { get; set; }

            public Branch Branch { get; set; }

            public int LocationId { get; set; }

            public Location Location { get; set; }

            public HashSet<WarehouseProduct> Products { get; set; } = new HashSet<WarehouseProduct>();

            public HashSet<WarehouseTag> Tags { get; set; } = new HashSet<WarehouseTag>();
        }

        public class ProductTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int ProductId { get; set; }

            public Product Product { get; set; }
        }

        public class Product : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public HashSet<WarehouseProduct> Warehouses { get; set; } = new HashSet<WarehouseProduct>();

            public HashSet<ProductTag> Tags { get; set; } = new HashSet<ProductTag>();
        }

        public class MovementTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int MovementId { get; set; }

            public Movement Movement { get; set; }
        }

        public class Movement : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public HashSet<MovementTag> Tags { get; set; } = new HashSet<MovementTag>();
        }

        public class AssociateTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int AssociateId { get; set; }

            public Associate Associate { get; set; }
        }

        public class Associate : EntityBase
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public HashSet<AssociateTag> Tags { get; set; } = new HashSet<AssociateTag>();
        }

        public class WarehouseProductTag
        {
            public int TagId { get; set; }

            public Tag Tag { get; set; }

            public int WarehouseProductId { get; set; }

            public WarehouseProduct WarehouseProduct { get; set; }
        }

        public class WarehouseProduct : EntityBase
        {
            public int WarehouseId { get; set; }

            public Warehouse Warehouse { get; set; }

            public int ProductId { get; set; }

            public Product Product { get; set; }

            public HashSet<WarehouseProductTag> Tags { get; set; } = new HashSet<WarehouseProductTag>();
        }
    }
}
