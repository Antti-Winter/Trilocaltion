using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class S2ConverterTests
    {
        [Fact]
        public void ToS2CellId_ReturnsNonZero()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong s2CellId = S2Converter.ToS2CellId(location, 15);
            Assert.NotEqual(0UL, s2CellId);
        }

        [Fact]
        public void ToS2CellId_DifferentLevels_DifferentValues()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong s2Level10 = S2Converter.ToS2CellId(location, 10);
            ulong s2Level20 = S2Converter.ToS2CellId(location, 20);
            Assert.NotEqual(s2Level10, s2Level20);
        }

        [Fact]
        public void S2_RoundTrip_Helsinki()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            ulong s2CellId = S2Converter.ToS2CellId(original, 18);
            var roundTripped = S2Converter.FromS2CellId(s2CellId, 10);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // S2 level 18 ≈ 38m × 38m cell
            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void S2_RoundTrip_Equator()
        {
            var original = new TriLocation(0.01, 0.01, 8);
            ulong s2CellId = S2Converter.ToS2CellId(original, 15);
            var roundTripped = S2Converter.FromS2CellId(s2CellId, 8);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void S2_RoundTrip_SouthernHemisphere()
        {
            var original = new TriLocation(-33.87, 151.21, 8);
            ulong s2CellId = S2Converter.ToS2CellId(original, 18);
            var roundTripped = S2Converter.FromS2CellId(s2CellId, 8);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }
    }
}
