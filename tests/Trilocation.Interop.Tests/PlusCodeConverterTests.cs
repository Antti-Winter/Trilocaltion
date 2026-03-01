using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class PlusCodeConverterTests
    {
        // === Format tests ===

        [Fact]
        public void ToPlusCode_ContainsPlusSign()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string code = PlusCodeConverter.ToPlusCode(location, 10);
            Assert.Contains("+", code);
        }

        [Fact]
        public void ToPlusCode_Length10_ProducesCorrectFormat()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string code = PlusCodeConverter.ToPlusCode(location, 10);
            // Length 10 code should be 11 chars total (8 + '+' + 2)
            Assert.Equal(11, code.Length);
            Assert.Equal('+', code[8]);
        }

        [Fact]
        public void ToPlusCode_OnlyValidCharacters()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string code = PlusCodeConverter.ToPlusCode(location, 10);
            string validChars = "23456789CFGHJMPQRVWX+";
            foreach (char c in code)
            {
                Assert.Contains(c, validChars);
            }
        }

        // === Known value tests ===

        [Fact]
        public void ToPlusCode_Helsinki_StartsWithCorrectPrefix()
        {
            // Helsinki (60.17, 24.94) should start with "9G"
            var location = new TriLocation(60.17, 24.94, 10);
            string code = PlusCodeConverter.ToPlusCode(location, 10);
            Assert.StartsWith("9G", code);
        }

        // === Round-trip tests ===

        [Theory]
        [InlineData(60.17, 24.94, 10, 10)]   // Helsinki
        [InlineData(0.0, 0.0, 5, 10)]         // Equator
        [InlineData(-33.87, 151.21, 8, 10)]   // Sydney
        [InlineData(40.71, -74.01, 8, 10)]    // New York
        public void PlusCode_RoundTrip_PreservesLocation(
            double lat, double lon, int resolution, int length)
        {
            var original = new TriLocation(lat, lon, resolution);
            string code = PlusCodeConverter.ToPlusCode(original, length);
            var roundTripped = PlusCodeConverter.FromPlusCode(code, resolution);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Length 10 ≈ 13.7m × 13.7m cell
            double maxErrorDeg = 0.001;
            Assert.True(Math.Abs(lat1 - lat2) < maxErrorDeg,
                "Lat error " + Math.Abs(lat1 - lat2) + " exceeds " + maxErrorDeg);
            Assert.True(Math.Abs(lon1 - lon2) < maxErrorDeg,
                "Lon error " + Math.Abs(lon1 - lon2) + " exceeds " + maxErrorDeg);
        }

        [Fact]
        public void PlusCode_ShorterLength_LargerError()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string code8 = PlusCodeConverter.ToPlusCode(location, 8);
            string code10 = PlusCodeConverter.ToPlusCode(location, 10);
            // Shorter code should be a prefix of the longer code (before the +)
            string prefix8 = code8.Replace("+", "");
            string prefix10 = code10.Replace("+", "");
            Assert.StartsWith(prefix8, prefix10);
        }

        [Fact]
        public void PlusCode_RoundTrip_Length8()
        {
            var original = new TriLocation(48.86, 2.35, 8);
            string code = PlusCodeConverter.ToPlusCode(original, 8);
            var roundTripped = PlusCodeConverter.FromPlusCode(code, 8);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Length 8 ≈ 275m × 275m cell
            Assert.True(Math.Abs(lat1 - lat2) < 0.01,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.01,
                "Lon error " + Math.Abs(lon1 - lon2));
        }
    }
}
