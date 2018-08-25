namespace AgileObjects.AgileMapper.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using static WhenMappingCircularReferences;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenMappingConcurrently
    {
        // See https://github.com/agileobjects/AgileMapper/issues/86
        [Fact]
        public void ShouldConcurrentlyMapLargeObjectsUsingTheStaticApi()
        {
            Parallel.For(
                0,
                Environment.ProcessorCount,
                i =>
                {
                    var counts = new Counts(100, 100);

                    var branch = Mapper.Map(CreateSourceBranch(counts)).ToANew<Issue77.Branch>();

                    branch.Warehouses.Count.ShouldBe(counts.Warehouses);

                    var product = default(Issue77.Product);

                    foreach (var warehouse in branch.Warehouses)
                    {
                        warehouse.Branch.ShouldBeSameAs(branch);
                        warehouse.Products.Count.ShouldBe(counts.WarehouseProducts);

                        foreach (var warehouseProduct in warehouse.Products)
                        {
                            if (product == null)
                            {
                                product = warehouseProduct.Product;
                            }
                            else
                            {
                                warehouseProduct.Product.ShouldBeSameAs(product);
                            }

                            warehouseProduct.Warehouse.ShouldBeSameAs(warehouse);
                        }
                    }

                    product.ShouldNotBeNull();
                    // ReSharper disable once PossibleNullReferenceException
                    product.Warehouses.Count.ShouldBe(counts.Warehouses * counts.WarehouseProducts);
                });
        }

        #region Helper Members

        private struct Counts
        {
            public Counts(int warehouses, int warehouseProducts)
            {
                Warehouses = warehouses;
                WarehouseProducts = warehouseProducts;
            }

            public int Warehouses { get; }

            public int WarehouseProducts { get; }
        }

        private static Issue77.Branch CreateSourceBranch(Counts counts)
        {
            var branch = new Issue77.Branch();
            var product = new Issue77.Product();

            for (var i = 0; i < counts.Warehouses; ++i)
            {
                branch.Warehouses.Add(new Issue77.Warehouse());
            }

            foreach (var warehouse in branch.Warehouses)
            {
                warehouse.Branch = branch;

                for (var i = 0; i < counts.WarehouseProducts; ++i)
                {
                    var warehouseProduct = new Issue77.WarehouseProduct
                    {
                        Product = product,
                        Warehouse = warehouse
                    };

                    product.Warehouses.Add(warehouseProduct);
                    warehouse.Products.Add(warehouseProduct);
                }
            }

            return branch;
        }

        #endregion
    }
}
