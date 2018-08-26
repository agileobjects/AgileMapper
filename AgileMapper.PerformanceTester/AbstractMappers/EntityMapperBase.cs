namespace AgileObjects.AgileMapper.PerformanceTester.AbstractMappers
{
    using System;
    using System.Diagnostics;
    using static TestClasses.Entities;

    internal abstract class EntityMapperBase : MapperTestBase
    {
        private readonly Warehouse _warehouse;

        public override int NumberOfExecutions => 100_000;

        protected EntityMapperBase()
        {
            var warehouse = new Warehouse
            {
                Id = 16473,
                Name = "Test Warehouse 16473",
                Description = "The test warehouse"
            };

            var tagForWarehouse = new Tag
            {
                Id = 46437
            };

            var warehouseTag = new WarehouseTag
            {
                WarehouseId = warehouse.Id,
                Warehouse = warehouse,
                TagId = tagForWarehouse.Id,
                Tag = tagForWarehouse
            };

            var branch = new Branch
            {
                Id = 27362,
                Name = "Test Branch 27362",
                Description = "The test branch"
            };

            var tagForBranch = new Tag
            {
                Id = 57832
            };

            var branchTag = new BranchTag
            {
                BranchId = branch.Id,
                Branch = branch,
                TagId = tagForBranch.Id,
                Tag = tagForBranch
            };

            var warehouseLocation = new Location
            {
                Id = 63672,
                Name = "Warehouse Location",
                Description = "Warehouse Street, Warehouse Land"
            };

            var tagForWarehouseLocation = new Tag
            {
                Id = 53627
            };

            var warehouseLocationTag = new LocationTag
            {
                LocationId = warehouseLocation.Id,
                Location = warehouseLocation,
                TagId = tagForWarehouseLocation.Id,
                Tag = tagForWarehouseLocation
            };

            var branchLocation = new Location
            {
                Id = 73726,
                Name = "Branch Location",
                Description = "Branch Street, Branch Land"
            };

            var tagForBranchLocation = new Tag
            {
                Id = 53272
            };

            var branchLocationTag = new LocationTag
            {
                LocationId = branchLocation.Id,
                Location = branchLocation,
                TagId = tagForBranchLocation.Id,
                Tag = tagForBranchLocation
            };

            var product = new Product
            {
                Id = 37638,
                Name = "Test Product",
                Description = "The test product"
            };

            var tagForProduct = new Tag
            {
                Id = 58276
            };

            var productTag = new ProductTag
            {
                ProductId = product.Id,
                Product = product,
                TagId = tagForProduct.Id,
                Tag = tagForProduct
            };

            var warehouseProduct = new WarehouseProduct
            {
                Id = 38376,
                WarehouseId = warehouse.Id,
                Warehouse = warehouse,
                ProductId = product.Id,
                Product = product
            };

            var tagForWarehouseProduct = new Tag
            {
                Id = 63463
            };

            var warehouseProductTag = new WarehouseProductTag
            {
                WarehouseProductId = warehouseProduct.Id,
                WarehouseProduct = warehouseProduct,
                TagId = tagForWarehouseProduct.Id,
                Tag = tagForWarehouseProduct
            };

            warehouse.BranchId = branch.Id;
            warehouse.Branch = branch;
            warehouse.LocationId = warehouseLocation.Id;
            warehouse.Location = warehouseLocation;
            warehouse.Tags.Add(warehouseTag);
            tagForWarehouse.Warehouses.Add(warehouseTag);
            tagForWarehouseLocation.Locations.Add(warehouseLocationTag);
            warehouseLocation.Tags.Add(warehouseLocationTag);

            branch.Location = branchLocation;
            branch.Warehouses.Add(warehouse);
            branch.Tags.Add(branchTag);
            tagForBranch.Branches.Add(branchTag);
            tagForBranchLocation.Locations.Add(branchLocationTag);
            branchLocation.Tags.Add(branchLocationTag);

            product.Tags.Add(productTag);
            tagForProduct.Products.Add(productTag);

            warehouse.Products.Add(warehouseProduct);
            product.Warehouses.Add(warehouseProduct);
            warehouseProduct.Tags.Add(warehouseProductTag);
            tagForWarehouseProduct.WarehouseProducts.Add(warehouseProductTag);

            _warehouse = warehouse;
        }

        public override object Execute(Stopwatch timer) => Clone(_warehouse);

        protected abstract Warehouse Clone(Warehouse warehouse);

        public override void Verify(object result)
        {
        }
    }
}
