using Microsoft.EntityFrameworkCore;

namespace Trilocation.Data.Tests
{
    public class TriModelBuilderExtensionsTests
    {
        private TestDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            var context = new TestDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public void ConfigureTriIndex_AddsValueConverter()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            var property = entityType.FindProperty(nameof(TestEntity.TriIndex))!;
            Assert.NotNull(property.GetValueConverter());
            Assert.IsType<TriIndexValueConverter>(property.GetValueConverter());
        }

        [Fact]
        public void ConfigureTriIndex_CreatesIndex()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            var indices = entityType.GetIndexes().ToList();
            bool hasTriIndexIndex = indices.Any(idx =>
                idx.Properties.Any(p => p.Name == nameof(TestEntity.TriIndex)));
            Assert.True(hasTriIndexIndex);
        }

        [Fact]
        public void ConfigureTriIndex_SetsColumnType()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(typeof(TestEntity))!;
            var property = entityType.FindProperty(nameof(TestEntity.TriIndex))!;
            var columnType = property.GetColumnType();
            Assert.Equal("BIGINT", columnType);
        }
    }
}
