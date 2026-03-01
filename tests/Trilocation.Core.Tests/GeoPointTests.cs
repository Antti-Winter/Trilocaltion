using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class GeoPointTests
    {
        [Fact]
        public void Constructor_StoresCoordinates()
        {
            var point = new GeoPoint(60.1699, 24.9384);
            Assert.Equal(60.1699, point.Latitude);
            Assert.Equal(24.9384, point.Longitude);
        }

        [Fact]
        public void DistanceTo_HelsinkiToTallinn_Approximately80Km()
        {
            var helsinki = new GeoPoint(60.1699, 24.9384);
            var tallinn = new GeoPoint(59.4370, 24.7536);

            double distanceM = helsinki.DistanceTo(tallinn);

            // Helsinki-Tallinna noin 80 km, toleranssi 5 km
            Assert.InRange(distanceM, 75_000, 85_000);
        }

        [Fact]
        public void DistanceTo_SamePoint_ReturnsZero()
        {
            var point = new GeoPoint(60.1699, 24.9384);
            Assert.Equal(0.0, point.DistanceTo(point), 1e-6);
        }

        [Fact]
        public void DistanceTo_Antipodal_ApproximatelyHalfCircumference()
        {
            var north = new GeoPoint(0.0, 0.0);
            var south = new GeoPoint(0.0, 180.0);

            double distanceM = north.DistanceTo(south);

            // Puolikas kehasta ~ 20015 km, toleranssi 100 km
            Assert.InRange(distanceM, 19_900_000, 20_100_000);
        }

        [Fact]
        public void MidpointTo_EquatorPoints_ReturnsMidpoint()
        {
            var a = new GeoPoint(0.0, 0.0);
            var b = new GeoPoint(0.0, 10.0);

            GeoPoint mid = a.MidpointTo(b);

            Assert.Equal(0.0, mid.Latitude, 1e-6);
            Assert.Equal(5.0, mid.Longitude, 1e-4);
        }

        [Fact]
        public void Equals_SameCoordinates_ReturnsTrue()
        {
            var a = new GeoPoint(60.0, 24.0);
            var b = new GeoPoint(60.0, 24.0);
            Assert.Equal(a, b);
        }

        [Fact]
        public void Equals_DifferentCoordinates_ReturnsFalse()
        {
            var a = new GeoPoint(60.0, 24.0);
            var b = new GeoPoint(61.0, 24.0);
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            var point = new GeoPoint(60.5, 24.5);
            string result = point.ToString();
            Assert.Contains("60", result);
            Assert.Contains("24", result);
            Assert.StartsWith("(", result);
            Assert.EndsWith(")", result);
        }
    }
}
