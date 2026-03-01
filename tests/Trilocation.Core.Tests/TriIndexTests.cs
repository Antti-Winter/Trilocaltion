using Xunit;
using Trilocation.Core.Primitives;
using Trilocation.Core.Indexing;

namespace Trilocation.Core.Tests
{
    public class TriIndexTests
    {
        // === FromLatLon / FromIndex ===

        [Fact]
        public void FromLatLon_CreatesCorrectTriLocation()
        {
            TriLocation loc = TriIndex.FromLatLon(60.17, 24.94, 5);

            Assert.Equal(5, loc.Resolution);
        }

        [Fact]
        public void FromLatLon_MatchesTriLocationConstructor()
        {
            TriLocation fromIndex = new TriLocation(60.17, 24.94, 5);
            TriLocation fromFacade = TriIndex.FromLatLon(60.17, 24.94, 5);

            Assert.Equal(fromIndex.Index, fromFacade.Index);
        }

        [Fact]
        public void FromIndex_CreatesCorrectTriLocation()
        {
            TriLocation loc = TriIndex.FromIndex(74);

            Assert.Equal(74UL, loc.Index);
            Assert.Equal(2, loc.Resolution);
        }

        // === FromBounds ===

        [Fact]
        public void FromBounds_WholeWorld_Level0_ReturnsEight()
        {
            GeoBounds world = new GeoBounds(-90.0, 90.0, -180.0, 180.0);
            TriLocation[] result = TriIndex.FromBounds(world, 0);

            Assert.Equal(8, result.Length);
        }

        [Fact]
        public void FromBounds_SmallArea_ReturnsTriangles()
        {
            // Helsinki area
            GeoBounds bounds = new GeoBounds(60.0, 60.5, 24.5, 25.5);
            TriLocation[] result = TriIndex.FromBounds(bounds, 3);

            Assert.True(result.Length > 0);
            Assert.All(result, loc => Assert.Equal(3, loc.Resolution));
        }

        [Fact]
        public void FromBounds_AllResultsOverlapBounds()
        {
            GeoBounds bounds = new GeoBounds(60.0, 60.5, 24.5, 25.5);
            TriLocation[] result = TriIndex.FromBounds(bounds, 2);

            foreach (var loc in result)
            {
                var cell = loc.ToCell();
                var cellBounds = cell.GetBounds();
                Assert.True(bounds.Intersects(cellBounds));
            }
        }

        // === CumulativeCount / LevelStart / LevelEnd / GetResolution ===

        [Fact]
        public void CumulativeCount_DelegatesCorrectly()
        {
            Assert.Equal(CumulativeIndex.CumulativeCount(3), TriIndex.CumulativeCount(3));
        }

        [Fact]
        public void LevelStart_DelegatesCorrectly()
        {
            Assert.Equal(CumulativeIndex.LevelStart(2), TriIndex.LevelStart(2));
        }

        [Fact]
        public void LevelEnd_DelegatesCorrectly()
        {
            Assert.Equal(CumulativeIndex.LevelEnd(2), TriIndex.LevelEnd(2));
        }

        [Fact]
        public void GetResolution_DelegatesCorrectly()
        {
            Assert.Equal(CumulativeIndex.GetResolution(74), TriIndex.GetResolution(74));
        }

        // === Hierarchy ===

        [Fact]
        public void GetParent_DelegatesCorrectly()
        {
            TriLocation loc = new TriLocation(74);
            TriLocation parent = TriIndex.GetParent(loc);

            Assert.Equal(loc.GetParent().Index, parent.Index);
        }

        [Fact]
        public void GetChildren_DelegatesCorrectly()
        {
            TriLocation loc = new TriLocation(17);
            TriLocation[] children = TriIndex.GetChildren(loc);

            Assert.Equal(4, children.Length);
            Assert.Equal(
                loc.GetChildren().Select(c => c.Index).OrderBy(i => i),
                children.Select(c => c.Index).OrderBy(i => i));
        }

        [Fact]
        public void GetDescendants_DelegatesCorrectly()
        {
            TriLocation loc = new TriLocation(3);
            TriLocation[] descendants = TriIndex.GetDescendants(loc, 2);

            Assert.Equal(20, descendants.Length);
        }

        // === Neighbors ===

        [Fact]
        public void GetNeighbors_DelegatesCorrectly()
        {
            TriLocation loc = new TriLocation(9);
            TriLocation[] neighbors = TriIndex.GetNeighbors(loc);

            Assert.Equal(3, neighbors.Length);
            Assert.Equal(
                loc.GetNeighbors().Select(n => n.Index).OrderBy(i => i),
                neighbors.Select(n => n.Index).OrderBy(i => i));
        }

        [Fact]
        public void GetRing_DelegatesCorrectly()
        {
            TriLocation loc = new TriLocation(9);
            TriLocation[] ring = TriIndex.GetRing(loc, 1);

            Assert.Equal(3, ring.Length);
        }

        // === Distance ===

        [Fact]
        public void GetDistance_SameTriangle_ReturnsSmallValue()
        {
            TriLocation loc = new TriLocation(9);
            double distance = TriIndex.GetDistance(loc, loc);

            Assert.Equal(0.0, distance);
        }

        [Fact]
        public void GetGridDistance_SameTriangle_ReturnsZero()
        {
            TriLocation loc = new TriLocation(9);
            int distance = TriIndex.GetGridDistance(loc, loc);

            Assert.Equal(0, distance);
        }

        // === IsValid ===

        [Fact]
        public void IsValid_ValidIndex_ReturnsTrue()
        {
            Assert.True(TriIndex.IsValid(1));
            Assert.True(TriIndex.IsValid(8));
            Assert.True(TriIndex.IsValid(74));
        }

        [Fact]
        public void IsValid_ZeroIndex_ReturnsFalse()
        {
            Assert.False(TriIndex.IsValid(0));
        }
    }
}
