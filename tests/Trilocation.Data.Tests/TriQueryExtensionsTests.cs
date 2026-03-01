using Microsoft.EntityFrameworkCore;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace Trilocation.Data.Tests
{
    public class TriQueryExtensionsTests : IDisposable
    {
        private readonly TestDbContext _context;

        public TriQueryExtensionsTests()
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

        private void SeedEntities(params ulong[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                _context.Entities.Add(new TestEntity
                {
                    Id = i + 1,
                    Name = "Entity" + (i + 1),
                    TriIndex = indices[i]
                });
            }
            _context.SaveChanges();
        }

        // === WithinArea tests ===

        [Fact]
        public void WithinArea_ReturnsDescendantsAtSameLevel()
        {
            // Ancestor at level 0 (face 0, index 1)
            var ancestor = new TriLocation(1);
            // Get children (level 1 descendants)
            ulong[] children = CumulativeIndex.GetChildren(1);

            // Seed: 4 children of face 0 + 1 child of face 1
            ulong[] face1Children = CumulativeIndex.GetChildren(2);
            SeedEntities(children[0], children[1], children[2], children[3], face1Children[0]);

            var result = _context.Entities.WithinArea(ancestor).ToList();

            // Should include 4 children of face 0, exclude face 1's child
            Assert.Equal(4, result.Count);
            Assert.All(result, e => Assert.Contains(e.TriIndex, children));
        }

        [Fact]
        public void WithinArea_IncludesAncestorItself()
        {
            var ancestor = new TriLocation(1);
            SeedEntities(1);

            var result = _context.Entities.WithinArea(ancestor).ToList();

            Assert.Single(result);
            Assert.Equal(1UL, result[0].TriIndex);
        }

        [Fact]
        public void WithinArea_ExcludesOutsideEntities()
        {
            var ancestor = new TriLocation(1); // face 0
            // Seed only entities from face 1 (index 2) descendants
            ulong[] face1Children = CumulativeIndex.GetChildren(2);
            SeedEntities(face1Children[0], face1Children[1]);

            var result = _context.Entities.WithinArea(ancestor).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void WithinArea_EmptyDatabase_ReturnsEmpty()
        {
            var ancestor = new TriLocation(1);
            var result = _context.Entities.WithinArea(ancestor).ToList();
            Assert.Empty(result);
        }

        [Fact]
        public void WithinArea_DeepDescendants()
        {
            // Ancestor at level 1
            ulong[] level1 = CumulativeIndex.GetChildren(1);
            var ancestor = new TriLocation(level1[0]); // first child of face 0

            // Get level 2 descendants (grandchildren of face 0, children of level1[0])
            ulong[] level2 = CumulativeIndex.GetChildren(level1[0]);
            // And a non-descendant at level 2 (child of level1[1])
            ulong[] otherLevel2 = CumulativeIndex.GetChildren(level1[1]);

            SeedEntities(level2[0], level2[1], level2[2], level2[3], otherLevel2[0]);

            var result = _context.Entities.WithinArea(ancestor).ToList();

            Assert.Equal(4, result.Count);
            Assert.All(result, e => Assert.Contains(e.TriIndex, level2));
        }

        // === ChildrenOf tests ===

        [Fact]
        public void ChildrenOf_ReturnsFourChildren()
        {
            var parent = new TriLocation(1);
            ulong[] children = CumulativeIndex.GetChildren(1);
            SeedEntities(children[0], children[1], children[2], children[3]);

            var result = _context.Entities.ChildrenOf(parent).ToList();

            Assert.Equal(4, result.Count);
        }

        [Fact]
        public void ChildrenOf_ExcludesNonChildren()
        {
            var parent = new TriLocation(1);
            ulong[] children = CumulativeIndex.GetChildren(1);
            ulong[] otherChildren = CumulativeIndex.GetChildren(2);

            // Seed children of face 0 + children of face 1
            SeedEntities(children[0], children[1], children[2], children[3],
                otherChildren[0], otherChildren[1]);

            var result = _context.Entities.ChildrenOf(parent).ToList();

            Assert.Equal(4, result.Count);
            Assert.All(result, e => Assert.Contains(e.TriIndex, children));
        }

        [Fact]
        public void ChildrenOf_ExcludesParentItself()
        {
            var parent = new TriLocation(1);
            ulong[] children = CumulativeIndex.GetChildren(1);
            SeedEntities(1, children[0], children[1], children[2], children[3]);

            var result = _context.Entities.ChildrenOf(parent).ToList();

            // Should return only 4 children, not the parent
            Assert.Equal(4, result.Count);
            Assert.DoesNotContain(result, e => e.TriIndex == 1);
        }

        // === AtResolution tests ===

        [Fact]
        public void AtResolution_ReturnsCorrectLevel()
        {
            // Seed entities at level 0 and level 1
            ulong[] level1 = CumulativeIndex.GetChildren(1);
            SeedEntities(1, 2, 3, level1[0], level1[1]);

            var result = _context.Entities.AtResolution(0).ToList();

            // Should return only level 0 entities (1, 2, 3)
            Assert.Equal(3, result.Count);
            Assert.All(result, e => Assert.True(e.TriIndex >= 1 && e.TriIndex <= 8));
        }

        [Fact]
        public void AtResolution_ExcludesOtherLevels()
        {
            ulong[] level1 = CumulativeIndex.GetChildren(1);
            SeedEntities(level1[0], level1[1], level1[2], level1[3]);

            var result = _context.Entities.AtResolution(0).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void AtResolution_Level1()
        {
            ulong[] level1Face0 = CumulativeIndex.GetChildren(1);
            ulong[] level1Face1 = CumulativeIndex.GetChildren(2);
            SeedEntities(1, level1Face0[0], level1Face0[1], level1Face1[0]);

            var result = _context.Entities.AtResolution(1).ToList();

            Assert.Equal(3, result.Count);
            Assert.All(result, e =>
            {
                ulong start = CumulativeIndex.LevelStart(1);
                ulong end = CumulativeIndex.LevelEnd(1);
                Assert.True(e.TriIndex >= start && e.TriIndex <= end);
            });
        }

        // === NearBy tests ===

        [Fact]
        public void NearBy_ReturnsWiderArea()
        {
            // Create entity at level 2
            ulong[] level1 = CumulativeIndex.GetChildren(1);
            ulong[] level2 = CumulativeIndex.GetChildren(level1[0]);
            var center = new TriLocation(level2[0]); // level 2 triangle

            // Seed: the center entity and a sibling at level 2
            SeedEntities(level2[0], level2[1]);

            // NearBy with expandResolution=1 → ancestor at level 1
            var result = _context.Entities.NearBy(center, 1).ToList();

            // Should include both entities (both are descendants of level1[0])
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void NearBy_ExcludesDistantEntities()
        {
            ulong[] level1 = CumulativeIndex.GetChildren(1);
            ulong[] level2A = CumulativeIndex.GetChildren(level1[0]);
            ulong[] level2B = CumulativeIndex.GetChildren(level1[1]);
            var center = new TriLocation(level2A[0]);

            // Seed: one nearby (same parent), one distant (different parent)
            SeedEntities(level2A[0], level2B[0]);

            // NearBy with expandResolution=1 → ancestor is level1[0]
            var result = _context.Entities.NearBy(center, 1).ToList();

            Assert.Single(result);
            Assert.Equal(level2A[0], result[0].TriIndex);
        }
    }
}
