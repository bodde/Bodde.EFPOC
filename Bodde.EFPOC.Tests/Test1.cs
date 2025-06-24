using AutoMapper;
using Bodde.EFPOC.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using static Azure.Core.HttpHeader;

namespace Bodde.EFPOC.Tests
{
    [TestClass]
    public sealed class Test1
    {
        private const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=Bodde.EFPOC;Integrated Security=True;Persist Security Info=False;Pooling=False;Multiple Active Result Sets=False;Encrypt=True;Trust Server Certificate=False;Command Timeout=0";
        private IMapper _mapper;

        [TestInitialize]
        public void TestInitialize()
        {
            // Ensure the database is created before each test
            using (var ctx = CreateDbContext())
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }

            // Updated AutoMapper configuration
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProjectProfile>();
            });

            // Initialize the Mapper instance
            _mapper = config.CreateMapper();
        }


        [TestMethod]
        public void AddProjectWithProducts()
        {
            #region Arrange

            using var ctx = ArrangeData(
                projectNames: [],
                productNames: ["X", "Y", "Z"], 
                projectProductNames: []
                );

            var dto = new ProjectUpsertDto
            {
                Name = "new",
                ProductIds = GetProjectIds(ctx, "X", "Y")
            };

            #endregion

            #region Act

            var entity = _mapper.Map<Project>(dto);
            ctx.Projects.Add(entity);
            ctx.SaveChanges();

            #endregion

            #region Assert

            var actual = ctx.Projects.Include(_ => _.ProjectProducts).Single(p => p.Id == entity.Id);

            var actualProductIds = actual.ProjectProducts
                .Select(pp => pp.ProductId)
                .ToArray();

            Assert.AreEqual(dto.Name, actual.Name);
            Assert.AreEqual(dto.ProductIds.ToCsv(), actualProductIds.ToCsv());

            #endregion
        }


        [TestMethod]
        public void AddProductsToProject()
        {
            #region Arrange

            using var ctx = ArrangeData(
                projectNames: ["A"],
                productNames: ["X", "Y", "Z"],
                projectProductNames: [("A", "X")]
                );


            var dto = new ProjectUpsertDto
            {
                Name = $"A_changed",
                ProductIds = GetProductIds(ctx, "X", "Y", "Z")  // expecting Y and Z to be added
            };

            #endregion

            #region Act

            var entity = ctx.Projects
                .Include(_ => _.ProjectProducts)
                .Single(_ => _.Name == "A");

            _mapper.Map(dto, entity);

            ctx.Projects.Update(entity);

            ctx.SaveChanges();

            #endregion

            #region Assert

            var actual = ctx.Projects.Include(_ => _.ProjectProducts).Single(p => p.Id == entity.Id);

            var actualProductIds = actual.ProjectProducts
                .Select(pp => pp.ProductId)
                .ToArray();

            Assert.AreEqual(dto.Name, actual.Name);
            Assert.AreEqual(dto.ProductIds.ToCsv(), actualProductIds.ToCsv());

            #endregion
        }

        [TestMethod]
        public void RemoveProductFromProject()
        {
            #region Arrange

            using var ctx = ArrangeData(
                projectNames: ["A"],
                productNames: ["X", "Y", "Z"],
                projectProductNames: [("A", "X"), ("A", "Y"), ("A", "Z")]
                );

            var dto = new ProjectUpsertDto
            {
                Name = $"A_changed",
                ProductIds = GetProductIds(ctx, "X", "Z")   // expecting Y to be removed
            };

            #endregion

            #region Act

            var entity = ctx.Projects
                .Include(_ => _.ProjectProducts)
                .Single(_ => _.Name == "A");

            _mapper.Map(dto, entity);

            ctx.Projects.Update(entity);

            ctx.SaveChanges();

            #endregion

            #region Assert

            var actual = ctx.Projects.Include(_ => _.ProjectProducts).Single(p => p.Id == entity.Id);

            var actualProductIds = actual.ProjectProducts
                .Select(pp => pp.ProductId)
                .ToArray();

            Assert.AreEqual(dto.Name, actual.Name);
            Assert.AreEqual(dto.ProductIds.ToCsv(), actualProductIds.ToCsv());

            #endregion
        }

        [TestMethod]
        public void RemoveProjectWithProducts()
        {
            #region Arrange

            using var ctx = ArrangeData(
                projectNames: ["A"],
                productNames: ["X", "Y", "Z"],
                projectProductNames: [("A", "X"), ("A", "Y"), ("A", "Z")]
                );

            #endregion

            #region Act

            var entity = ctx.Projects
                .Include(_ => _.ProjectProducts)
                .Single(_ => _.Name == "A");

            ctx.Projects.Remove(entity);

            ctx.SaveChanges();

            #endregion

            #region Assert

            var actual = ctx.Projects.Include(_ => _.ProjectProducts).SingleOrDefault(p => p.Id == entity.Id);

            Assert.IsNull(actual);
            Assert.IsFalse(ctx.ProjectProducts.Any());

            #endregion
        }

        private MyDbContext CreateDbContext()
        {
            var opt = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new MyDbContext(opt);
        }

        private MyDbContext ArrangeData(
            string[] projectNames, 
            string[] productNames,
            (string, string)[] projectProductNames)
        {
            using (var ctx = CreateDbContext())
            {

                var projects = BuildProjects(projectNames);
                ctx.Projects.AddRange(projects);

                var products = BuildProducts(productNames);
                ctx.Products.AddRange(products);

                var projectProducts = AddProductsToProjects(projects, products, projectProductNames);
                ctx.ProjectProducts.AddRange(projectProducts);

                ctx.SaveChanges();
            }

            // use another db context to ensure the data is available for the tests
            return CreateDbContext();
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

        private int[] GetProjectIds(MyDbContext ctx, params string[] names)
        {
            return ctx.Projects
                .Where(_ => names.Contains(_.Name))
                .Select(_ => _.Id)
                .ToArray();
        }

        private int[] GetProductIds(MyDbContext ctx, params string[] names)
        {
            return ctx.Products
                .Where(_ => names.Contains(_.Name))
                .Select(_ => _.Id)
                .ToArray();
        }
    }
}
