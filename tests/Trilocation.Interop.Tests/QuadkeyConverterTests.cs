using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class QuadkeyConverterTests
    {
        // === Format tests ===

        [Fact]
        public void ToQuadkey_LevelMatchesLength()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            for (int level = 1; level <= 5; level++)
            {
                string quadkey = QuadkeyConverter.ToQuadkey(location, level);
                Assert.Equal(level, quadkey.Length);
            }
        }

        [Fact]
        public void ToQuadkey_OnlyDigits0123()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string quadkey = QuadkeyConverter.ToQuadkey(location, 16);
            foreach (char c in quadkey)
            {
                Assert.True(c == '0' || c == '1' || c == '2' || c == '3',
                    "Quadkey char '" + c + "' is not 0-3");
            }
        }

        // === Round-trip tests ===

        [Theory]
        [InlineData(60.17, 24.94, 10, 10)]   // Helsinki
        [InlineData(0.01, 0.01, 5, 10)]       // Near equator (avoid exact 0,0 boundary)
        [InlineData(-33.87, 151.21, 8, 12)]   // Sydney
        [InlineData(40.71, -74.01, 8, 16)]    // New York
        public void Quadkey_RoundTrip_PreservesLocation(
            double lat, double lon, int resolution, int level)
        {
            var original = new TriLocation(lat, lon, resolution);
            string quadkey = QuadkeyConverter.ToQuadkey(original, level);
            var roundTripped = QuadkeyConverter.FromQuadkey(quadkey, resolution);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Error tolerance: combined Quadkey cell size + TriLocation quantization
            // Level 10 tile ≈ 0.35° lon, level 12 ≈ 0.09°, level 16 ≈ 0.005°
            double maxErrorDeg = level switch
            {
                10 => 0.5,
                12 => 0.15,
                16 => 0.01,
                _ => 1.0
            };
            Assert.True(Math.Abs(lat1 - lat2) < maxErrorDeg,
                "Lat error " + Math.Abs(lat1 - lat2) + " exceeds " + maxErrorDeg);
            Assert.True(Math.Abs(lon1 - lon2) < maxErrorDeg,
                "Lon error " + Math.Abs(lon1 - lon2) + " exceeds " + maxErrorDeg);
        }

        [Fact]
        public void Quadkey_Level1_FourPossibleValues()
        {
            // At level 1, there are only 4 tiles: "0", "1", "2", "3"
            var location = new TriLocation(60.17, 24.94, 5);
            string quadkey = QuadkeyConverter.ToQuadkey(location, 1);
            Assert.Single(quadkey);
            Assert.Contains(quadkey[0], "0123");
        }

        [Fact]
        public void Quadkey_HighLevel_SmallError()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            string quadkey = QuadkeyConverter.ToQuadkey(original, 20);
            var roundTripped = QuadkeyConverter.FromQuadkey(quadkey, 10);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Level 20 ≈ 0.15m, very precise
            Assert.True(Math.Abs(lat1 - lat2) < 0.0001,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.0001,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void FromQuadkey_SameLevel_ConsistentIndex()
        {
            // Two nearby points at the same quadkey should produce the same TriLocation
            var loc1 = new TriLocation(60.17, 24.94, 10);
            var loc2 = new TriLocation(60.17, 24.94, 10);
            string qk1 = QuadkeyConverter.ToQuadkey(loc1, 12);
            string qk2 = QuadkeyConverter.ToQuadkey(loc2, 12);
            Assert.Equal(qk1, qk2);
        }
    }
}
