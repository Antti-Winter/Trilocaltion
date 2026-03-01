using Trilocation.Core.Geometry;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class Vector3DTests
    {
        private const double Epsilon = 1e-10;

        // === Normalize ===

        [Fact]
        public void Normalize_UnitVector_RemainsUnit()
        {
            var v = new Vector3D(1.0, 0.0, 0.0);
            Vector3D n = v.Normalize();
            Assert.Equal(1.0, n.Length(), Epsilon);
        }

        [Fact]
        public void Normalize_ArbitraryVector_ReturnsUnitLength()
        {
            var v = new Vector3D(3.0, 4.0, 0.0);
            Vector3D n = v.Normalize();
            Assert.Equal(1.0, n.Length(), Epsilon);
        }

        [Fact]
        public void Normalize_PreservesDirection()
        {
            var v = new Vector3D(2.0, 0.0, 0.0);
            Vector3D n = v.Normalize();
            Assert.Equal(1.0, n.X, Epsilon);
            Assert.Equal(0.0, n.Y, Epsilon);
            Assert.Equal(0.0, n.Z, Epsilon);
        }

        // === Dot ===

        [Fact]
        public void Dot_PerpendicularVectors_ReturnsZero()
        {
            var a = new Vector3D(1.0, 0.0, 0.0);
            var b = new Vector3D(0.0, 1.0, 0.0);
            Assert.Equal(0.0, a.Dot(b), Epsilon);
        }

        [Fact]
        public void Dot_ParallelVectors_ReturnsProduct()
        {
            var a = new Vector3D(2.0, 0.0, 0.0);
            var b = new Vector3D(3.0, 0.0, 0.0);
            Assert.Equal(6.0, a.Dot(b), Epsilon);
        }

        [Fact]
        public void Dot_OppositeVectors_ReturnsNegative()
        {
            var a = new Vector3D(1.0, 0.0, 0.0);
            var b = new Vector3D(-1.0, 0.0, 0.0);
            Assert.Equal(-1.0, a.Dot(b), Epsilon);
        }

        // === Cross ===

        [Fact]
        public void Cross_XandY_ReturnsZ()
        {
            var x = new Vector3D(1.0, 0.0, 0.0);
            var y = new Vector3D(0.0, 1.0, 0.0);
            Vector3D result = x.Cross(y);
            Assert.Equal(0.0, result.X, Epsilon);
            Assert.Equal(0.0, result.Y, Epsilon);
            Assert.Equal(1.0, result.Z, Epsilon);
        }

        [Fact]
        public void Cross_ParallelVectors_ReturnsZero()
        {
            var a = new Vector3D(1.0, 0.0, 0.0);
            var b = new Vector3D(2.0, 0.0, 0.0);
            Vector3D result = a.Cross(b);
            Assert.Equal(0.0, result.Length(), Epsilon);
        }

        // === Length ===

        [Fact]
        public void Length_UnitVector_ReturnsOne()
        {
            var v = new Vector3D(1.0, 0.0, 0.0);
            Assert.Equal(1.0, v.Length(), Epsilon);
        }

        [Fact]
        public void Length_345Triangle_Returns5()
        {
            var v = new Vector3D(3.0, 4.0, 0.0);
            Assert.Equal(5.0, v.Length(), Epsilon);
        }

        // === FromLatLon / ToLatLon ===

        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(60.1699, 24.9384)]
        [InlineData(-33.8688, 151.2093)]
        [InlineData(90.0, 0.0)]
        [InlineData(-90.0, 0.0)]
        [InlineData(0.0, 180.0)]
        [InlineData(0.0, -180.0)]
        [InlineData(45.0, -90.0)]
        public void FromLatLon_ToLatLon_RoundTrip(double lat, double lon)
        {
            Vector3D v = Vector3D.FromLatLon(lat, lon);
            (double resultLat, double resultLon) = v.ToLatLon();

            Assert.Equal(lat, resultLat, 1e-10);
            // Longitude voi olla +180 tai -180, molemmat ovat sama
            if (Math.Abs(lon) == 180.0)
            {
                Assert.True(
                    Math.Abs(resultLon - 180.0) < 1e-10
                    || Math.Abs(resultLon + 180.0) < 1e-10);
            }
            else
            {
                Assert.Equal(lon, resultLon, 1e-10);
            }
        }

        [Fact]
        public void FromLatLon_ReturnsUnitVector()
        {
            Vector3D v = Vector3D.FromLatLon(60.1699, 24.9384);
            Assert.Equal(1.0, v.Length(), Epsilon);
        }

        [Fact]
        public void FromLatLon_NorthPole_PointsUp()
        {
            Vector3D v = Vector3D.FromLatLon(90.0, 0.0);
            Assert.Equal(0.0, v.X, Epsilon);
            Assert.Equal(0.0, v.Y, Epsilon);
            Assert.Equal(1.0, v.Z, Epsilon);
        }

        [Fact]
        public void FromLatLon_Equator0_PointsAlongX()
        {
            Vector3D v = Vector3D.FromLatLon(0.0, 0.0);
            Assert.Equal(1.0, v.X, Epsilon);
            Assert.Equal(0.0, v.Y, Epsilon);
            Assert.Equal(0.0, v.Z, Epsilon);
        }

        // === Operators ===

        [Fact]
        public void OperatorAdd_AddsComponents()
        {
            var a = new Vector3D(1.0, 2.0, 3.0);
            var b = new Vector3D(4.0, 5.0, 6.0);
            Vector3D result = a + b;
            Assert.Equal(5.0, result.X, Epsilon);
            Assert.Equal(7.0, result.Y, Epsilon);
            Assert.Equal(9.0, result.Z, Epsilon);
        }

        [Fact]
        public void OperatorSubtract_SubtractsComponents()
        {
            var a = new Vector3D(4.0, 5.0, 6.0);
            var b = new Vector3D(1.0, 2.0, 3.0);
            Vector3D result = a - b;
            Assert.Equal(3.0, result.X, Epsilon);
            Assert.Equal(3.0, result.Y, Epsilon);
            Assert.Equal(3.0, result.Z, Epsilon);
        }

        [Fact]
        public void OperatorMultiplyScalar_ScalesComponents()
        {
            var v = new Vector3D(1.0, 2.0, 3.0);
            Vector3D result = v * 2.0;
            Assert.Equal(2.0, result.X, Epsilon);
            Assert.Equal(4.0, result.Y, Epsilon);
            Assert.Equal(6.0, result.Z, Epsilon);
        }
    }
}
