using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class GeoBoundsTests
    {
        [Fact]
        public void Constructor_StoresBounds()
        {
            var bounds = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            Assert.Equal(59.0, bounds.MinLatitude);
            Assert.Equal(61.0, bounds.MaxLatitude);
            Assert.Equal(24.0, bounds.MinLongitude);
            Assert.Equal(26.0, bounds.MaxLongitude);
        }

        [Fact]
        public void Contains_PointInside_ReturnsTrue()
        {
            var bounds = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            var point = new GeoPoint(60.0, 25.0);
            Assert.True(bounds.Contains(point));
        }

        [Fact]
        public void Contains_PointOutside_ReturnsFalse()
        {
            var bounds = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            var point = new GeoPoint(62.0, 25.0);
            Assert.False(bounds.Contains(point));
        }

        [Fact]
        public void Contains_PointOnEdge_ReturnsTrue()
        {
            var bounds = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            var point = new GeoPoint(59.0, 24.0);
            Assert.True(bounds.Contains(point));
        }

        [Fact]
        public void Intersects_OverlappingBounds_ReturnsTrue()
        {
            var a = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            var b = new GeoBounds(60.0, 62.0, 25.0, 27.0);
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void Intersects_NonOverlapping_ReturnsFalse()
        {
            var a = new GeoBounds(59.0, 60.0, 24.0, 25.0);
            var b = new GeoBounds(61.0, 62.0, 26.0, 27.0);
            Assert.False(a.Intersects(b));
        }

        [Fact]
        public void Intersects_TouchingEdge_ReturnsTrue()
        {
            var a = new GeoBounds(59.0, 60.0, 24.0, 25.0);
            var b = new GeoBounds(60.0, 61.0, 25.0, 26.0);
            Assert.True(a.Intersects(b));
        }

        [Fact]
        public void Equals_SameBounds_ReturnsTrue()
        {
            var a = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            var b = new GeoBounds(59.0, 61.0, 24.0, 26.0);
            Assert.Equal(a, b);
        }
    }
}
