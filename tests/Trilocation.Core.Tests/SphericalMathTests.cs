using Trilocation.Core.Geometry;
using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class SphericalMathTests
    {
        private const double Tolerance = 1e-10;
        private const double AngleTolerance = 1e-6;

        // === SphericalDistance ===

        [Fact]
        public void SphericalDistance_SamePoint_ReturnsZero()
        {
            Vector3D point = Vector3D.FromLatLon(60.17, 24.94);
            double distance = SphericalMath.SphericalDistance(point, point);
            Assert.Equal(0.0, distance, Tolerance);
        }

        [Fact]
        public void SphericalDistance_AntipodalPoints_ReturnsPi()
        {
            Vector3D northPole = new Vector3D(0, 0, 1);
            Vector3D southPole = new Vector3D(0, 0, -1);
            double distance = SphericalMath.SphericalDistance(northPole, southPole);
            Assert.Equal(Math.PI, distance, Tolerance);
        }

        [Fact]
        public void SphericalDistance_QuarterSphere_ReturnsHalfPi()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            double distance = SphericalMath.SphericalDistance(a, b);
            Assert.Equal(Math.PI / 2.0, distance, Tolerance);
        }

        [Fact]
        public void SphericalDistance_HelsinkiTallinn_ApproxCorrect()
        {
            Vector3D helsinki = Vector3D.FromLatLon(60.17, 24.94);
            Vector3D tallinn = Vector3D.FromLatLon(59.44, 24.75);
            double distanceRad = SphericalMath.SphericalDistance(helsinki, tallinn);
            double distanceKm = distanceRad * GeoConstants.EarthRadiusKm;
            Assert.InRange(distanceKm, 75.0, 85.0);
        }

        [Fact]
        public void SphericalDistance_HelsinkiTokyo_ApproxCorrect()
        {
            Vector3D helsinki = Vector3D.FromLatLon(60.17, 24.94);
            Vector3D tokyo = Vector3D.FromLatLon(35.68, 139.69);
            double distanceRad = SphericalMath.SphericalDistance(helsinki, tokyo);
            double distanceKm = distanceRad * GeoConstants.EarthRadiusKm;
            Assert.InRange(distanceKm, 7700.0, 7900.0);
        }

        [Fact]
        public void SphericalDistance_IsSymmetric()
        {
            Vector3D a = Vector3D.FromLatLon(45.0, 10.0);
            Vector3D b = Vector3D.FromLatLon(-30.0, 120.0);
            double d1 = SphericalMath.SphericalDistance(a, b);
            double d2 = SphericalMath.SphericalDistance(b, a);
            Assert.Equal(d1, d2, Tolerance);
        }

        // === Slerp ===

        [Fact]
        public void Slerp_AtZero_ReturnsStart()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            Vector3D result = SphericalMath.Slerp(a, b, 0.0);
            Assert.Equal(a.X, result.X, Tolerance);
            Assert.Equal(a.Y, result.Y, Tolerance);
            Assert.Equal(a.Z, result.Z, Tolerance);
        }

        [Fact]
        public void Slerp_AtOne_ReturnsEnd()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            Vector3D result = SphericalMath.Slerp(a, b, 1.0);
            Assert.Equal(b.X, result.X, Tolerance);
            Assert.Equal(b.Y, result.Y, Tolerance);
            Assert.Equal(b.Z, result.Z, Tolerance);
        }

        [Fact]
        public void Slerp_AtHalf_ReturnsMidpoint()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            Vector3D result = SphericalMath.Slerp(a, b, 0.5);
            double expectedComponent = Math.Sqrt(2.0) / 2.0;
            Assert.Equal(expectedComponent, result.X, Tolerance);
            Assert.Equal(expectedComponent, result.Y, Tolerance);
            Assert.Equal(0.0, result.Z, Tolerance);
        }

        [Fact]
        public void Slerp_ResultIsNormalized()
        {
            Vector3D a = Vector3D.FromLatLon(60.0, 25.0);
            Vector3D b = Vector3D.FromLatLon(-33.0, 151.0);
            Vector3D result = SphericalMath.Slerp(a, b, 0.3);
            Assert.Equal(1.0, result.Length(), Tolerance);
        }

        [Fact]
        public void Slerp_SamePoint_ReturnsSamePoint()
        {
            Vector3D a = Vector3D.FromLatLon(45.0, 90.0);
            Vector3D result = SphericalMath.Slerp(a, a, 0.5);
            Assert.Equal(a.X, result.X, Tolerance);
            Assert.Equal(a.Y, result.Y, Tolerance);
            Assert.Equal(a.Z, result.Z, Tolerance);
        }

        // === MidpointOnSphere ===

        [Fact]
        public void MidpointOnSphere_IsNormalized()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 0, 1);
            Vector3D mid = SphericalMath.MidpointOnSphere(a, b);
            Assert.Equal(1.0, mid.Length(), Tolerance);
        }

        [Fact]
        public void MidpointOnSphere_EquidistantFromBoth()
        {
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            Vector3D mid = SphericalMath.MidpointOnSphere(a, b);
            double distA = SphericalMath.SphericalDistance(a, mid);
            double distB = SphericalMath.SphericalDistance(b, mid);
            Assert.Equal(distA, distB, Tolerance);
        }

        [Fact]
        public void MidpointOnSphere_MatchesSlerp()
        {
            Vector3D a = Vector3D.FromLatLon(60.0, 25.0);
            Vector3D b = Vector3D.FromLatLon(40.0, -74.0);
            Vector3D midSlerp = SphericalMath.Slerp(a, b, 0.5);
            Vector3D midDirect = SphericalMath.MidpointOnSphere(a, b);
            Assert.Equal(midSlerp.X, midDirect.X, Tolerance);
            Assert.Equal(midSlerp.Y, midDirect.Y, Tolerance);
            Assert.Equal(midSlerp.Z, midDirect.Z, Tolerance);
        }

        // === SphericalExcess ===

        [Fact]
        public void SphericalExcess_RightAngleTriangle_ReturnsHalfPi()
        {
            // Triangle with three 90-degree angles: area = pi/2
            Vector3D a = new Vector3D(1, 0, 0);
            Vector3D b = new Vector3D(0, 1, 0);
            Vector3D c = new Vector3D(0, 0, 1);
            double excess = SphericalMath.SphericalExcess(a, b, c);
            Assert.Equal(Math.PI / 2.0, excess, AngleTolerance);
        }

        [Fact]
        public void SphericalExcess_FullOctant_ReturnsHalfPi()
        {
            // Same as above but different vertex order - should be same area
            Vector3D a = new Vector3D(0, 0, 1);
            Vector3D b = new Vector3D(1, 0, 0);
            Vector3D c = new Vector3D(0, 1, 0);
            double excess = SphericalMath.SphericalExcess(a, b, c);
            Assert.Equal(Math.PI / 2.0, excess, AngleTolerance);
        }

        [Fact]
        public void SphericalExcess_IsAlwaysPositive()
        {
            Vector3D a = Vector3D.FromLatLon(60.0, 25.0);
            Vector3D b = Vector3D.FromLatLon(59.0, 24.0);
            Vector3D c = Vector3D.FromLatLon(59.5, 26.0);
            double excess = SphericalMath.SphericalExcess(a, b, c);
            Assert.True(excess > 0);
        }

        [Fact]
        public void SphericalExcess_EightOctants_CoverFullSphere()
        {
            // 8 octants of the sphere, each has area pi/2
            // Total = 8 * pi/2 = 4*pi (full sphere)
            Vector3D px = new Vector3D(1, 0, 0);
            Vector3D py = new Vector3D(0, 1, 0);
            Vector3D pz = new Vector3D(0, 0, 1);
            Vector3D nx = new Vector3D(-1, 0, 0);
            Vector3D ny = new Vector3D(0, -1, 0);
            Vector3D nz = new Vector3D(0, 0, -1);

            double totalArea = 0;
            totalArea += SphericalMath.SphericalExcess(pz, px, py);
            totalArea += SphericalMath.SphericalExcess(pz, py, nx);
            totalArea += SphericalMath.SphericalExcess(pz, nx, ny);
            totalArea += SphericalMath.SphericalExcess(pz, ny, px);
            totalArea += SphericalMath.SphericalExcess(nz, py, px);
            totalArea += SphericalMath.SphericalExcess(nz, nx, py);
            totalArea += SphericalMath.SphericalExcess(nz, ny, nx);
            totalArea += SphericalMath.SphericalExcess(nz, px, ny);

            Assert.Equal(4.0 * Math.PI, totalArea, AngleTolerance);
        }
    }
}
