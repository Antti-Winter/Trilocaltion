using Microsoft.EntityFrameworkCore;

namespace Trilocation.Data.Tests
{
    public class TestDbContext : DbContext
    {
        public DbSet<TestEntity> Entities { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>().ConfigureTriIndex();
        }
    }
}
