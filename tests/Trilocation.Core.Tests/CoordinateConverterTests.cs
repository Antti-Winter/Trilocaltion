using Trilocation.Core.Algorithms;
using Trilocation.Core.Geometry;
using Trilocation.Core.Indexing;
using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class CoordinateConverterTests
    {
        // === Encoding: ToIndex ===

        [Fact]
        public void ToIndex_Resolution0_ReturnsBaseFaceIndex()
        {
            // Any point should return an index between 1 and 8 at resolution 0
            ulong index = CoordinateConverter.ToIndex(60.17, 24.94, 0);
            Assert.InRange(index, 1UL, 8UL);
        }

        [Fact]
        public void ToIndex_Resolution1_ReturnsValidIndex()
        {
            ulong index = CoordinateConverter.ToIndex(60.17, 24.94, 1);
            Assert.InRange(index, 9UL, 40UL);
        }

        [Fact]
        public void ToIndex_SamePointSameResolution_ReturnsSameIndex()
        {
            ulong index1 = CoordinateConverter.ToIndex(60.17, 24.94, 15);
            ulong index2 = CoordinateConverter.ToIndex(60.17, 24.94, 15);
            Assert.Equal(index1, index2);
        }

        [Fact]
        public void ToIndex_CorrectResolutionLevel()
        {
            for (int res = 0; res <= 5; res++)
            {
                ulong index = CoordinateConverter.ToIndex(60.17, 24.94, res);
                int detectedRes = CumulativeIndex.GetResolution(index);
                Assert.Equal(res, detectedRes);
            }
        }

        [Fact]
        public void ToIndex_NorthPole_ReturnsNorthernFace()
        {
            ulong index = CoordinateConverter.ToIndex(89.99, 0.0, 0);
            int baseFace = CumulativeIndex.GetBaseFace(index);
            Assert.InRange(baseFace, 0, 3);
        }

        [Fact]
        public void ToIndex_SouthPole_ReturnsSouthernFace()
        {
            ulong index = CoordinateConverter.ToIndex(-89.99, 0.0, 0);
            int baseFace = CumulativeIndex.GetBaseFace(index);
            Assert.InRange(baseFace, 4, 7);
        }

        [Fact]
        public void ToIndex_Equator_ReturnsValidIndex()
        {
            ulong index = CoordinateConverter.ToIndex(0.0, 0.0, 10);
            Assert.True(IndexValidator.IsValid(index));
        }

        [Theory]
        [InlineData(60.17, 24.94)]    // Helsinki
        [InlineData(51.51, -0.13)]    // London
        [InlineData(-33.87, 151.21)]  // Sydney
        [InlineData(35.68, 139.69)]   // Tokyo
        [InlineData(-22.91, -43.17)]  // Rio de Janeiro
        [InlineData(0.0, 0.0)]        // Null Island
        public void ToIndex_VariousPoints_ReturnsValidIndices(double lat, double lon)
        {
            ulong index = CoordinateConverter.ToIndex(lat, lon, 15);
            Assert.True(IndexValidator.IsValid(index));
            Assert.Equal(15, CumulativeIndex.GetResolution(index));
        }

        // === Decoding: ToLatLon ===

        [Fact]
        public void ToLatLon_BaseFace_ReturnsValidCoordinates()
        {
            var (lat, lon) = CoordinateConverter.ToLatLon(1);
            Assert.InRange(lat, -90.0, 90.0);
            Assert.InRange(lon, -180.0, 180.0);
        }

        [Fact]
        public void ToLatLon_AllBaseFaces_ReturnDifferentCoordinates()
        {
            var points = new (double Lat, double Lon)[8];
            for (ulong i = 1; i <= 8; i++)
            {
                points[i - 1] = CoordinateConverter.ToLatLon(i);
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    bool same = Math.Abs(points[i].Lat - points[j].Lat) < 0.01
                        && Math.Abs(points[i].Lon - points[j].Lon) < 0.01;
                    Assert.False(same,
                        "Base faces " + (i + 1) + " and " + (j + 1) + " have same centroid");
                }
            }
        }

        // === Round-trip ===

        [Theory]
        [InlineData(60.17, 24.94, 5)]
        [InlineData(60.17, 24.94, 10)]
        [InlineData(60.17, 24.94, 15)]
        [InlineData(60.17, 24.94, 20)]
        [InlineData(51.51, -0.13, 10)]
        [InlineData(51.51, -0.13, 20)]
        [InlineData(-33.87, 151.21, 10)]
        [InlineData(-33.87, 151.21, 20)]
        [InlineData(0.0, 0.0, 15)]
        [InlineData(89.99, 0.0, 10)]
        [InlineData(-89.99, 0.0, 10)]
        public void RoundTrip_ErrorDecreasesWithResolution(double lat, double lon, int resolution)
        {
            ulong index = CoordinateConverter.ToIndex(lat, lon, resolution);
            var (decodedLat, decodedLon) = CoordinateConverter.ToLatLon(index);

            GeoPoint original = new GeoPoint(lat, lon);
            GeoPoint decoded = new GeoPoint(decodedLat, decodedLon);
            double errorMeters = original.DistanceTo(decoded);

            // Error should be less than circumradius at this resolution
            // Earth surface area / triangle count gives approximate triangle area
            // circumradius ~ sqrt(area) approximately
            double triangleCount = 8.0 * Math.Pow(4, resolution);
            double triangleAreaM2 = GeoConstants.EarthSurfaceAreaKm2 * 1e6 / triangleCount;
            double approxCircumradius = Math.Sqrt(triangleAreaM2);

            Assert.True(errorMeters < approxCircumradius,
                "Round-trip error " + errorMeters + "m exceeds circumradius " + approxCircumradius + "m at resolution " + resolution);
        }

        [Fact]
        public void RoundTrip_Helsinki_Resolution24_ErrorLessThanHalfMeter()
        {
            // Circumradius at level 24 is ~0.42m, but centroid offset can slightly exceed it.
            // Use 0.5m as practical limit.
            ulong index = CoordinateConverter.ToIndex(60.17, 24.94, 24);
            var (decodedLat, decodedLon) = CoordinateConverter.ToLatLon(index);

            GeoPoint original = new GeoPoint(60.17, 24.94);
            GeoPoint decoded = new GeoPoint(decodedLat, decodedLon);
            double errorMeters = original.DistanceTo(decoded);

            Assert.True(errorMeters < 0.50,
                "Resolution 24 error " + errorMeters + "m should be < 0.50m");
        }

        // === GetTriangle ===

        [Fact]
        public void GetTriangle_BaseFace_ReturnsOctantTriangle()
        {
            Triangle3D tri = CoordinateConverter.GetTriangle(1);
            Assert.True(tri.Area() > 0);
        }

        [Fact]
        public void GetTriangle_ChildContainedInParent()
        {
            ulong parentIndex = CoordinateConverter.ToIndex(60.17, 24.94, 5);
            ulong[] children = CumulativeIndex.GetChildren(parentIndex);

            Triangle3D parentTri = CoordinateConverter.GetTriangle(parentIndex);

            for (int i = 0; i < children.Length; i++)
            {
                Triangle3D childTri = CoordinateConverter.GetTriangle(children[i]);
                Assert.True(parentTri.Contains(childTri.Centroid),
                    "Child " + i + " centroid should be inside parent");
            }
        }

        // === Error handling ===

        [Fact]
        public void ToIndex_InvalidResolution_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CoordinateConverter.ToIndex(60.0, 25.0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                CoordinateConverter.ToIndex(60.0, 25.0, 31));
        }

        [Fact]
        public void ToLatLon_InvalidIndex_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                CoordinateConverter.ToLatLon(0));
        }
    }
}
