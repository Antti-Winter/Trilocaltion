using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class TriCellTests
    {
        // === Basic properties ===

        [Fact]
        public void ToCell_HasLocation()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();
            Assert.Equal(loc, cell.Location);
        }

        [Fact]
        public void ToCell_HasThreeDistinctVertices()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();

            Assert.False(cell.VertexA.Equals(cell.VertexB));
            Assert.False(cell.VertexB.Equals(cell.VertexC));
            Assert.False(cell.VertexA.Equals(cell.VertexC));
        }

        [Fact]
        public void ToCell_HasValidCentroid()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();

            Assert.InRange(cell.Centroid.Latitude, -90.0, 90.0);
            Assert.InRange(cell.Centroid.Longitude, -180.0, 180.0);
        }

        // === Contains ===

        [Fact]
        public void Contains_CentroidIsAlwaysInside()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();

            Assert.True(cell.Contains(cell.Centroid.Latitude, cell.Centroid.Longitude));
        }

        [Fact]
        public void Contains_FarAwayPoint_ReturnsFalse()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 15);
            TriCell cell = loc.ToCell();

            // Point on opposite side of Earth
            Assert.False(cell.Contains(-60.17, 24.94 + 180.0));
        }

        // === AreaKm2 ===

        [Fact]
        public void AreaKm2_Level0_ApproxOneEighthOfEarth()
        {
            TriLocation loc = new TriLocation(1);
            TriCell cell = loc.ToCell();

            double expectedArea = GeoConstants.EarthSurfaceAreaKm2 / 8.0;
            double ratio = cell.AreaKm2 / expectedArea;
            Assert.InRange(ratio, 0.9, 1.1);
        }

        [Fact]
        public void AreaKm2_HigherResolution_SmallerArea()
        {
            TriLocation loc5 = new TriLocation(60.17, 24.94, 5);
            TriLocation loc10 = new TriLocation(60.17, 24.94, 10);

            TriCell cell5 = loc5.ToCell();
            TriCell cell10 = loc10.ToCell();

            Assert.True(cell10.AreaKm2 < cell5.AreaKm2);
        }

        [Fact]
        public void AreaKm2_IsAlwaysPositive()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 15);
            TriCell cell = loc.ToCell();
            Assert.True(cell.AreaKm2 > 0);
        }

        // === GetBounds ===

        [Fact]
        public void GetBounds_ContainsAllVertices()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();
            GeoBounds bounds = cell.GetBounds();

            Assert.True(bounds.Contains(cell.VertexA));
            Assert.True(bounds.Contains(cell.VertexB));
            Assert.True(bounds.Contains(cell.VertexC));
        }

        [Fact]
        public void GetBounds_ContainsCentroid()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            TriCell cell = loc.ToCell();
            GeoBounds bounds = cell.GetBounds();

            Assert.True(bounds.Contains(cell.Centroid));
        }
    }
}
