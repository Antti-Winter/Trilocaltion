using Xunit;
using Trilocation.Core.Conversions;

namespace Trilocation.Core.Tests
{
    public class TriConvertTests
    {
        // === WGS84 round-trip ===

        [Fact]
        public void FromWgs84_DelegatesToTriLocation()
        {
            var location = TriConvert.FromWgs84(60.17, 24.94, 10);
            var direct = new TriLocation(60.17, 24.94, 10);
            Assert.Equal(direct.Index, location.Index);
        }

        [Fact]
        public void ToWgs84_DelegatesToTriLocation()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            var (lat, lon) = TriConvert.ToWgs84(location);
            var (directLat, directLon) = location.ToLatLon();
            Assert.Equal(directLat, lat);
            Assert.Equal(directLon, lon);
        }

        [Theory]
        [InlineData(0.0, 0.0, 5)]
        [InlineData(60.17, 24.94, 10)]
        [InlineData(-33.87, 151.21, 8)]
        [InlineData(89.99, 0.0, 3)]
        [InlineData(-89.99, 0.0, 3)]
        public void Wgs84_RoundTrip_ConsistentWithTriLocation(
            double lat, double lon, int resolution)
        {
            var location = TriConvert.FromWgs84(lat, lon, resolution);
            var (resultLat, resultLon) = TriConvert.ToWgs84(location);
            var directLocation = new TriLocation(lat, lon, resolution);
            var (directLat, directLon) = directLocation.ToLatLon();
            Assert.Equal(directLat, resultLat);
            Assert.Equal(directLon, resultLon);
        }

        // === WebMercator round-trip ===

        [Fact]
        public void WebMercator_Origin_MapsToOrigin()
        {
            var location = TriConvert.FromWebMercator(0.0, 0.0, 5);
            var (lat, lon) = TriConvert.ToWgs84(location);
            Assert.True(Math.Abs(lat) < 1.0);
            Assert.True(Math.Abs(lon) < 1.0);
        }

        [Fact]
        public void WebMercator_Helsinki_RoundTrip()
        {
            // Helsinki: lat 60.17, lon 24.94
            var location = TriConvert.FromWgs84(60.17, 24.94, 10);
            var (x, y) = TriConvert.ToWebMercator(location);
            var backLocation = TriConvert.FromWebMercator(x, y, 10);
            Assert.Equal(location.Index, backLocation.Index);
        }

        [Fact]
        public void ToWebMercator_KnownValues()
        {
            // Equator/prime meridian: should map to (0, 0)
            var location = TriConvert.FromWgs84(0.0, 0.0, 5);
            var (x, y) = TriConvert.ToWebMercator(location);
            // Centroid of the triangle containing (0,0), converted to Web Mercator
            // Just verify they are close to origin (within same quadrant)
            Assert.True(Math.Abs(x) < 3000000.0);
            Assert.True(Math.Abs(y) < 3000000.0);
        }

        [Theory]
        [InlineData(45.0, 90.0, 8)]
        [InlineData(-30.0, -60.0, 6)]
        [InlineData(0.0, 180.0, 5)]
        public void WebMercator_RoundTrip_PreservesIndex(
            double lat, double lon, int resolution)
        {
            var original = TriConvert.FromWgs84(lat, lon, resolution);
            var (x, y) = TriConvert.ToWebMercator(original);
            var roundTripped = TriConvert.FromWebMercator(x, y, resolution);
            Assert.Equal(original.Index, roundTripped.Index);
        }

        // === UTM round-trip ===

        [Fact]
        public void Utm_Helsinki_ZoneAndBand()
        {
            var location = TriConvert.FromWgs84(60.17, 24.94, 10);
            var (zone, band, easting, northing) = TriConvert.ToUtm(location);
            Assert.Equal(35, zone);
            Assert.Equal('V', band);
            Assert.True(easting > 0);
            Assert.True(northing > 0);
        }

        [Fact]
        public void Utm_Helsinki_RoundTrip()
        {
            var location = TriConvert.FromWgs84(60.17, 24.94, 10);
            var (zone, band, easting, northing) = TriConvert.ToUtm(location);
            var backLocation = TriConvert.FromUtm(zone, band, easting, northing, 10);
            Assert.Equal(location.Index, backLocation.Index);
        }

        [Theory]
        [InlineData(48.86, 2.35, 8)]    // Paris
        [InlineData(-33.87, 151.21, 8)]  // Sydney
        [InlineData(35.68, 139.69, 8)]   // Tokyo
        [InlineData(40.71, -74.01, 8)]   // New York
        public void Utm_RoundTrip_PreservesIndex(double lat, double lon, int resolution)
        {
            var original = TriConvert.FromWgs84(lat, lon, resolution);
            var (zone, band, easting, northing) = TriConvert.ToUtm(original);
            var roundTripped = TriConvert.FromUtm(zone, band, easting, northing, resolution);
            Assert.Equal(original.Index, roundTripped.Index);
        }

        [Fact]
        public void Utm_SouthernHemisphere_CorrectBand()
        {
            var location = TriConvert.FromWgs84(-33.87, 151.21, 5);
            var (zone, band, easting, northing) = TriConvert.ToUtm(location);
            // Sydney should be zone 56, band H
            Assert.Equal(56, zone);
            Assert.Equal('H', band);
        }

        // === MGRS round-trip ===

        [Fact]
        public void Mgrs_Helsinki_Format()
        {
            var location = TriConvert.FromWgs84(60.17, 24.94, 10);
            string mgrs = TriConvert.ToMgrs(location, 5);
            // MGRS should start with zone 35V
            Assert.StartsWith("35V", mgrs);
            // Precision 5 = 10 digits for easting+northing
            // Format: "35VLG4825111932" or similar (15 chars total)
            Assert.True(mgrs.Length >= 13);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public void Mgrs_RoundTrip_AtPrecision(int precision)
        {
            var original = TriConvert.FromWgs84(60.17, 24.94, 10);
            string mgrs = TriConvert.ToMgrs(original, precision);
            var backLocation = TriConvert.FromMgrs(mgrs, 10);
            // Round-trip error depends on precision:
            // Precision 5 = 1m, 3 = 100m, 1 = 10km
            var (lat1, lon1) = TriConvert.ToWgs84(original);
            var (lat2, lon2) = TriConvert.ToWgs84(backLocation);
            double maxErrorDeg = precision switch
            {
                5 => 0.001,
                3 => 0.01,
                1 => 0.5,
                _ => 1.0
            };
            Assert.True(Math.Abs(lat1 - lat2) < maxErrorDeg,
                "Latitude error " + Math.Abs(lat1 - lat2) + " exceeds " + maxErrorDeg);
            Assert.True(Math.Abs(lon1 - lon2) < maxErrorDeg,
                "Longitude error " + Math.Abs(lon1 - lon2) + " exceeds " + maxErrorDeg);
        }

        [Fact]
        public void Mgrs_Parse_ValidString()
        {
            // Known MGRS: 35VLG (Helsinki area grid square)
            var location = TriConvert.FromMgrs("35VLG4825111932", 10);
            var (lat, lon) = TriConvert.ToWgs84(location);
            // Should be near Helsinki
            Assert.True(lat > 55.0 && lat < 65.0, "Latitude " + lat + " not near Helsinki");
            Assert.True(lon > 20.0 && lon < 30.0, "Longitude " + lon + " not near Helsinki");
        }

        // === ECEF round-trip ===

        [Fact]
        public void Ecef_EquatorPrimeMeridian()
        {
            var location = TriConvert.FromWgs84(0.0, 0.0, 5);
            var (x, y, z) = TriConvert.ToEcef(location);
            // At equator, prime meridian: x ≈ Earth radius, y ≈ 0, z ≈ 0
            Assert.True(x > 6370000.0 && x < 6380000.0);
            Assert.True(Math.Abs(y) < 200000.0);
            Assert.True(Math.Abs(z) < 200000.0);
        }

        [Fact]
        public void Ecef_NorthPole()
        {
            var location = TriConvert.FromWgs84(89.99, 0.0, 3);
            var (x, y, z) = TriConvert.ToEcef(location);
            // At resolution 3, triangle centroid may be away from pole
            // but z should still be dominant (high latitude)
            double p = Math.Sqrt(x * x + y * y);
            Assert.True(z > p, "z=" + z + " should be > p=" + p + " near north pole");
            Assert.True(z > 5000000.0, "z=" + z + " should be large near north pole");
        }

        [Fact]
        public void Ecef_RoundTrip_PreservesIndex()
        {
            var original = TriConvert.FromWgs84(60.17, 24.94, 8);
            var (x, y, z) = TriConvert.ToEcef(original);
            var roundTripped = TriConvert.FromEcef(x, y, z, 8);
            Assert.Equal(original.Index, roundTripped.Index);
        }

        [Theory]
        [InlineData(0.0, 0.0, 5)]
        [InlineData(45.0, 90.0, 6)]
        [InlineData(-60.0, -120.0, 4)]
        [InlineData(89.99, 0.0, 3)]
        public void Ecef_RoundTrip_SmallLatLonError(double lat, double lon, int resolution)
        {
            var location = TriConvert.FromWgs84(lat, lon, resolution);
            var (x, y, z) = TriConvert.ToEcef(location);
            var backLocation = TriConvert.FromEcef(x, y, z, resolution);
            var (lat1, lon1) = TriConvert.ToWgs84(location);
            var (lat2, lon2) = TriConvert.ToWgs84(backLocation);
            Assert.True(Math.Abs(lat1 - lat2) < 1e-6,
                "Latitude error: " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 1e-6,
                "Longitude error: " + Math.Abs(lon1 - lon2));
        }
    }
}
