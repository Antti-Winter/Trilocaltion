using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class GeohashConverterTests
    {
        // === Known value tests ===

        [Fact]
        public void ToGeohash_Helsinki_StartsWithU()
        {
            // Helsinki (60.17, 24.94) should produce a geohash starting with 'u'
            var location = new TriLocation(60.17, 24.94, 10);
            string geohash = GeohashConverter.ToGeohash(location, 5);
            Assert.StartsWith("u", geohash);
        }

        [Fact]
        public void ToGeohash_Equator_StartsWithS()
        {
            // Equator/PM (0, 0) should produce a geohash starting with 's'
            var location = new TriLocation(0.0, 0.0, 5);
            string geohash = GeohashConverter.ToGeohash(location, 5);
            Assert.StartsWith("s", geohash);
        }

        [Fact]
        public void ToGeohash_PrecisionMatchesLength()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            for (int precision = 1; precision <= 9; precision++)
            {
                string geohash = GeohashConverter.ToGeohash(location, precision);
                Assert.Equal(precision, geohash.Length);
            }
        }

        [Fact]
        public void ToGeohash_OnlyBase32Characters()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string geohash = GeohashConverter.ToGeohash(location, 8);
            string validChars = "0123456789bcdefghjkmnpqrstuvwxyz";
            foreach (char c in geohash)
            {
                Assert.Contains(c, validChars);
            }
        }

        // === Round-trip tests ===

        [Theory]
        [InlineData(60.17, 24.94, 10, 5)]   // Helsinki
        [InlineData(0.0, 0.0, 5, 5)]         // Equator
        [InlineData(-33.87, 151.21, 8, 5)]   // Sydney
        [InlineData(40.71, -74.01, 8, 6)]    // New York
        public void Geohash_RoundTrip_PreservesLocation(
            double lat, double lon, int resolution, int precision)
        {
            var original = new TriLocation(lat, lon, resolution);
            string geohash = GeohashConverter.ToGeohash(original, precision);
            var roundTripped = GeohashConverter.FromGeohash(geohash, resolution);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Error tolerance depends on precision:
            // precision 5 ≈ 4.9km, precision 6 ≈ 1.2km, precision 8 ≈ 19m
            double maxErrorDeg = precision switch
            {
                5 => 0.1,
                6 => 0.02,
                8 => 0.001,
                _ => 0.5
            };
            Assert.True(Math.Abs(lat1 - lat2) < maxErrorDeg,
                "Lat error " + Math.Abs(lat1 - lat2) + " exceeds " + maxErrorDeg);
            Assert.True(Math.Abs(lon1 - lon2) < maxErrorDeg,
                "Lon error " + Math.Abs(lon1 - lon2) + " exceeds " + maxErrorDeg);
        }

        [Fact]
        public void FromGeohash_KnownValue_NearHelsinki()
        {
            // "ud" is a short geohash for the Helsinki area
            var location = GeohashConverter.FromGeohash("ud", 8);
            var (lat, lon) = location.ToLatLon();
            // Should be somewhere in Scandinavia/Finland area
            Assert.True(lat > 50.0 && lat < 70.0,
                "Latitude " + lat + " not in Scandinavia area");
            Assert.True(lon > 10.0 && lon < 50.0,
                "Longitude " + lon + " not in Scandinavia area");
        }

        [Fact]
        public void FromGeohash_HigherPrecision_RoundTrip()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            string geohash = GeohashConverter.ToGeohash(original, 9);
            var roundTripped = GeohashConverter.FromGeohash(geohash, 10);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Precision 9 ≈ 4.8m, should be very close
            Assert.True(Math.Abs(lat1 - lat2) < 0.001,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.001,
                "Lon error " + Math.Abs(lon1 - lon2));
        }
    }
}
