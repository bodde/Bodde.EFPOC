using Bodde.EFPOC.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bodde.EFPOC
{
    public class MyDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectProduct> ProjectProducts { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> opt) : base(opt) { }
    }
}
