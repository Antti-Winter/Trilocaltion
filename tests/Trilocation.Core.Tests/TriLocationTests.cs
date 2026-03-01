using Trilocation.Core.Indexing;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class TriLocationTests
    {
        // === Constructor from index ===

        [Fact]
        public void Constructor_Index_SetsIndex()
        {
            TriLocation loc = new TriLocation(17);
            Assert.Equal(17UL, loc.Index);
        }

        [Fact]
        public void Constructor_Index_SetsCorrectResolution()
        {
            TriLocation loc = new TriLocation(17);
            Assert.Equal(1, loc.Resolution);
        }

        [Fact]
        public void Constructor_Index_SetsCorrectBaseFace()
        {
            // Index 17: base face 2 (from projectplan example: 3 -> 17)
            TriLocation loc = new TriLocation(17);
            Assert.Equal(2, loc.BaseFace);
        }

        [Fact]
        public void Constructor_Index_BaseFaceLevel0()
        {
            for (ulong i = 1; i <= 8; i++)
            {
                TriLocation loc = new TriLocation(i);
                Assert.Equal(0, loc.Resolution);
                Assert.Equal((int)(i - 1), loc.BaseFace);
            }
        }

        [Fact]
        public void Constructor_InvalidIndex_Throws()
        {
            Assert.Throws<ArgumentException>(() => new TriLocation(0));
        }

        // === Constructor from coordinates ===

        [Fact]
        public void Constructor_LatLon_ReturnsValidLocation()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            Assert.Equal(10, loc.Resolution);
            Assert.True(IndexValidator.IsValid(loc.Index));
        }

        [Fact]
        public void Constructor_LatLon_SameAsCoordinateConverter()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 15);
            ulong directIndex = Algorithms.CoordinateConverter.ToIndex(60.17, 24.94, 15);
            Assert.Equal(directIndex, loc.Index);
        }

        // === ToLatLon ===

        [Fact]
        public void ToLatLon_ReturnsValidCoordinates()
        {
            TriLocation loc = new TriLocation(60.17, 24.94, 10);
            var (lat, lon) = loc.ToLatLon();
            Assert.InRange(lat, -90.0, 90.0);
            Assert.InRange(lon, -180.0, 180.0);
        }

        // === Equality ===

        [Fact]
        public void Equals_SameIndex_ReturnsTrue()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(17);
            Assert.True(a.Equals(b));
            Assert.True(a == b);
        }

        [Fact]
        public void Equals_DifferentIndex_ReturnsFalse()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(18);
            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }

        [Fact]
        public void GetHashCode_SameIndex_SameHash()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(17);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        // === CompareTo ===

        [Fact]
        public void CompareTo_SmallerIndex_ReturnsNegative()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(74);
            Assert.True(a.CompareTo(b) < 0);
            Assert.True(a < b);
        }

        [Fact]
        public void CompareTo_LargerIndex_ReturnsPositive()
        {
            TriLocation a = new TriLocation(74);
            TriLocation b = new TriLocation(17);
            Assert.True(a.CompareTo(b) > 0);
            Assert.True(a > b);
        }

        [Fact]
        public void CompareTo_SameIndex_ReturnsZero()
        {
            TriLocation a = new TriLocation(17);
            TriLocation b = new TriLocation(17);
            Assert.Equal(0, a.CompareTo(b));
        }

        // === ToString ===

        [Fact]
        public void ToString_ContainsIndexAndResolution()
        {
            TriLocation loc = new TriLocation(17);
            string str = loc.ToString();
            Assert.Contains("17", str);
            Assert.Contains("R1", str);
        }
    }
}
