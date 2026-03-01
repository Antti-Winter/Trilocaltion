using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class H3ConverterTests
    {
        [Fact]
        public void ToH3_ReturnsNonZero()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong h3Index = H3Converter.ToH3(location, 9);
            Assert.NotEqual(0UL, h3Index);
        }

        [Fact]
        public void ToH3_DifferentResolutions_DifferentValues()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            ulong h3Res5 = H3Converter.ToH3(location, 5);
            ulong h3Res9 = H3Converter.ToH3(location, 9);
            Assert.NotEqual(h3Res5, h3Res9);
        }

        [Fact]
        public void H3_RoundTrip_Helsinki()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            ulong h3Index = H3Converter.ToH3(original, 9);
            var roundTripped = H3Converter.FromH3(h3Index, 10);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // H3 resolution 9 ≈ 0.1 km² → ~0.3km side
            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void H3_RoundTrip_Equator()
        {
            var original = new TriLocation(0.01, 0.01, 8);
            ulong h3Index = H3Converter.ToH3(original, 7);
            var roundTripped = H3Converter.FromH3(h3Index, 8);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            Assert.True(Math.Abs(lat1 - lat2) < 0.05,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.05,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void H3_RoundTrip_SouthernHemisphere()
        {
            var original = new TriLocation(-33.87, 151.21, 8);
            ulong h3Index = H3Converter.ToH3(original, 9);
            var roundTripped = H3Converter.FromH3(h3Index, 8);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }
    }
}
