using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Bodde.EFPOC.Tests
{
    [TestClass]
    public sealed class Test1
    {
        private const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Bodde.EFPOC;Integrated Security=True;Persist Security Info=False;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=False;Command Timeout=0";

        [TestMethod]
        public void TestMethod1()
        {
            var opt = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            using (var ctx = new MyDbContext(opt))
            {
                var dbCreated = ctx.Database.EnsureCreated();
                if(dbCreated)
                {
                    SeedData(ctx);
                }
            }
        }

        private void SeedData(MyDbContext ctx)
        {
            var projects = BuildProjects("A", "B", "C");
            ctx.Projects.AddRange(projects);

            var products = BuildProducts("X", "Y", "Z");
            ctx.Products.AddRange(products);

            var projectProducts = AddProductsToProjects(
                projects, products, 
                ("A", "X"), 
                ("B", "X"), ("B", "Y")
                );
            ctx.ProjectProducts.AddRange(projectProducts);

            ctx.SaveChanges();
        }

        private Product[] BuildProducts(params string[] names)
        {
            return names
                .Select(name => new Product { Name = name })
                .ToArray();
        }

        private Project[] BuildProjects(params string[] names)
        {
            return names
                .Select(name => new Project { Name = name })
                .ToArray();
        }

        private ProjectProduct[] AddProductsToProjects(
            Project[] projects, Product[] products, 
            params (string, string)[] projectProductNames
            )
        {
           
            return projectProductNames
                .Select(_ => new ProjectProduct
                { 
                    Project = projects.Single(pj => pj.Name == _.Item1),
                    Product = products.Single(pr => pr.Name == _.Item2)
                })
                .ToArray();
        }

    }
}
