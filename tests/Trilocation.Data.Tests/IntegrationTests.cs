using Microsoft.EntityFrameworkCore;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace Trilocation.Data.Tests
{
    public class IntegrationTests : IDisposable
    {
        private readonly TestDbContext _context;

        public IntegrationTests()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;
            _context = new TestDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        // === CRUD tests ===

        [Fact]
        public void Create_AndRead_EntityWithTriIndex()
        {
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = 42 };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);

            Assert.Equal("Test", loaded.Name);
            Assert.Equal(42UL, loaded.TriIndex);
        }

        [Fact]
        public void Update_TriIndex()
        {
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = 42 };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            entity.TriIndex = 99;
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);
            Assert.Equal(99UL, loaded.TriIndex);
        }

        [Fact]
        public void Delete_EntityWithTriIndex()
        {
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = 42 };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.Entities.Remove(entity);
            _context.SaveChanges();

            Assert.Empty(_context.Entities.ToList());
        }

        // === Round-trip tests ===

        [Theory]
        [InlineData(0UL)]
        [InlineData(1UL)]
        [InlineData(8UL)]
        [InlineData(100UL)]
        [InlineData(1000000UL)]
        public void RoundTrip_SmallValues(ulong triIndex)
        {
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = triIndex };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);
            Assert.Equal(triIndex, loaded.TriIndex);
        }

        [Fact]
        public void RoundTrip_LongMaxValue()
        {
            ulong triIndex = (ulong)long.MaxValue;
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = triIndex };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);
            Assert.Equal(triIndex, loaded.TriIndex);
        }

        [Fact]
        public void RoundTrip_AboveLongMaxValue()
        {
            ulong triIndex = (ulong)long.MaxValue + 1;
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = triIndex };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);
            Assert.Equal(triIndex, loaded.TriIndex);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(20)]
        [InlineData(25)]
        [InlineData(29)]
        public void RoundTrip_CumulativeCountAtResolution(int resolution)
        {
            ulong triIndex = CumulativeIndex.CumulativeCount(resolution);
            var entity = new TestEntity { Id = 1, Name = "Test", TriIndex = triIndex };
            _context.Entities.Add(entity);
            _context.SaveChanges();

            _context.ChangeTracker.Clear();
            var loaded = _context.Entities.Single(e => e.Id == 1);
            Assert.Equal(triIndex, loaded.TriIndex);
        }

        // === Query integration tests ===

        [Fact]
        public void WithinArea_WorksWithRealData()
        {
            // Create a location at resolution 2 and seed its index
            var location = new TriLocation(60.17, 24.94, 2);
            var parent = location.GetParent(); // resolution 1

            // Seed: the location + a distant location
            var distantLocation = new TriLocation(-33.87, 151.21, 2); // Sydney
            _context.Entities.Add(new TestEntity { Id = 1, Name = "Helsinki", TriIndex = location.Index });
            _context.Entities.Add(new TestEntity { Id = 2, Name = "Sydney", TriIndex = distantLocation.Index });
            _context.SaveChanges();

            var result = _context.Entities.WithinArea(parent).ToList();

            Assert.Single(result);
            Assert.Equal("Helsinki", result[0].Name);
        }

        [Fact]
        public void MultipleQueries_CombineWithLinq()
        {
            // Seed entities at level 1
            ulong[] children = CumulativeIndex.GetChildren(1);
            _context.Entities.Add(new TestEntity { Id = 1, Name = "A", TriIndex = children[0] });
            _context.Entities.Add(new TestEntity { Id = 2, Name = "B", TriIndex = children[1] });
            _context.Entities.Add(new TestEntity { Id = 3, Name = "C", TriIndex = children[2] });
            _context.SaveChanges();

            // Combine spatial query with standard LINQ
            var result = _context.Entities
                .AtResolution(1)
                .Where(e => e.Name != "B")
                .ToList();

            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(result, e => e.Name == "B");
        }
    }
}
