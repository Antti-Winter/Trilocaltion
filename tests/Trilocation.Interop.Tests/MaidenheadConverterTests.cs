using Xunit;
using Trilocation.Core;
using Trilocation.Interop;

namespace Trilocation.Interop.Tests
{
    public class MaidenheadConverterTests
    {
        // === Known value tests ===

        [Fact]
        public void ToMaidenhead_Helsinki_StartsWithKP()
        {
            // Helsinki (60.17, 24.94): field K (lon), P (lat) → "KP"
            var location = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 1);
            Assert.StartsWith("KP", grid);
        }

        [Fact]
        public void ToMaidenhead_Helsinki_SquareKP20()
        {
            // Helsinki (60.17, 24.94): square "KP20"
            var location = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 2);
            Assert.StartsWith("KP20", grid);
        }

        [Fact]
        public void ToMaidenhead_London_StartsWithIO()
        {
            // London (51.5, -0.13): field I (lon), O (lat) → "IO"
            var location = new TriLocation(51.5, -0.13, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 1);
            Assert.StartsWith("IO", grid);
        }

        // === Format tests ===

        [Fact]
        public void ToMaidenhead_Precision1_TwoChars()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 1);
            Assert.Equal(2, grid.Length);
            Assert.True(char.IsUpper(grid[0]), "First char should be uppercase");
            Assert.True(char.IsUpper(grid[1]), "Second char should be uppercase");
        }

        [Fact]
        public void ToMaidenhead_Precision2_FourChars()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 2);
            Assert.Equal(4, grid.Length);
            Assert.True(char.IsUpper(grid[0]));
            Assert.True(char.IsUpper(grid[1]));
            Assert.True(char.IsDigit(grid[2]));
            Assert.True(char.IsDigit(grid[3]));
        }

        [Fact]
        public void ToMaidenhead_Precision3_SixChars()
        {
            var location = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(location, 3);
            Assert.Equal(6, grid.Length);
            Assert.True(char.IsLower(grid[4]), "5th char should be lowercase");
            Assert.True(char.IsLower(grid[5]), "6th char should be lowercase");
        }

        // === Round-trip tests ===

        [Theory]
        [InlineData(60.17, 24.94, 10, 2)]    // Helsinki, square
        [InlineData(51.5, -0.13, 10, 2)]      // London, square
        [InlineData(-33.87, 151.21, 8, 2)]    // Sydney, square
        public void Maidenhead_RoundTrip_Precision2(
            double lat, double lon, int resolution, int precision)
        {
            var original = new TriLocation(lat, lon, resolution);
            string grid = MaidenheadConverter.ToMaidenhead(original, precision);
            var roundTripped = MaidenheadConverter.FromMaidenhead(grid, resolution);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Precision 2 = square: 2° lon × 1° lat → generous tolerance
            Assert.True(Math.Abs(lat1 - lat2) < 1.5,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 2.5,
                "Lon error " + Math.Abs(lon1 - lon2));
        }

        [Fact]
        public void Maidenhead_RoundTrip_Precision3_SmallError()
        {
            var original = new TriLocation(60.17, 24.94, 10);
            string grid = MaidenheadConverter.ToMaidenhead(original, 3);
            var roundTripped = MaidenheadConverter.FromMaidenhead(grid, 10);

            var (lat1, lon1) = original.ToLatLon();
            var (lat2, lon2) = roundTripped.ToLatLon();

            // Precision 3 = subsquare: 5' lon × 2.5' lat ≈ 0.08° × 0.04°
            Assert.True(Math.Abs(lat1 - lat2) < 0.1,
                "Lat error " + Math.Abs(lat1 - lat2));
            Assert.True(Math.Abs(lon1 - lon2) < 0.15,
                "Lon error " + Math.Abs(lon1 - lon2));
        }
    }
}
