using Trilocation.Core.Geometry;
using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class Triangle3DTests
    {
        private const double Tolerance = 1e-10;
        private const double AreaTolerance = 1e-6;

        // Helper: create a standard octant triangle (north pole, equator 0, equator 90)
        private static Triangle3D CreateOctantTriangle()
        {
            return new Triangle3D(
                new Vector3D(0, 0, 1),
                new Vector3D(1, 0, 0),
                new Vector3D(0, 1, 0));
        }

        // === Constructor and Centroid ===

        [Fact]
        public void Constructor_SetVertices()
        {
            Vector3D a = new Vector3D(0, 0, 1);
            Vector3D b = new Vector3D(1, 0, 0);
            Vector3D c = new Vector3D(0, 1, 0);
            Triangle3D tri = new Triangle3D(a, b, c);

            Assert.Equal(a, tri.A);
            Assert.Equal(b, tri.B);
            Assert.Equal(c, tri.C);
        }

        [Fact]
        public void Centroid_IsNormalized()
        {
            Triangle3D tri = CreateOctantTriangle();
            Assert.Equal(1.0, tri.Centroid.Length(), Tolerance);
        }

        [Fact]
        public void Centroid_IsInsideTriangle()
        {
            Triangle3D tri = CreateOctantTriangle();
            Assert.True(tri.Contains(tri.Centroid));
        }

        // === Subdivide ===

        [Fact]
        public void Subdivide_ReturnsFourChildren()
        {
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();
            Assert.Equal(4, children.Length);
        }

        [Fact]
        public void Subdivide_ChildrenAreaSumEqualsParent()
        {
            Triangle3D tri = CreateOctantTriangle();
            double parentArea = tri.Area();
            Triangle3D[] children = tri.Subdivide();

            double childrenAreaSum = 0;
            for (int i = 0; i < children.Length; i++)
            {
                childrenAreaSum += children[i].Area();
            }

            Assert.Equal(parentArea, childrenAreaSum, AreaTolerance);
        }

        [Fact]
        public void Subdivide_ChildrenAreasAreApproxEqual()
        {
            // On the sphere, the center child is larger than corner children.
            // For an octant triangle the ratio is ~1.40 for center, ~0.87 for corners.
            // At deeper levels the ratio converges toward 1.0.
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();
            double parentArea = tri.Area();
            double expectedChildArea = parentArea / 4.0;

            for (int i = 0; i < children.Length; i++)
            {
                double childArea = children[i].Area();
                double ratio = childArea / expectedChildArea;
                Assert.InRange(ratio, 0.6, 1.5);
            }
        }

        [Fact]
        public void Subdivide_ChildCentroidsAreInsideParent()
        {
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();

            for (int i = 0; i < children.Length; i++)
            {
                Assert.True(tri.Contains(children[i].Centroid),
                    "Child " + i + " centroid should be inside parent");
            }
        }

        [Fact]
        public void Subdivide_ChildrenHaveNormalizedVertices()
        {
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();

            for (int i = 0; i < children.Length; i++)
            {
                Assert.Equal(1.0, children[i].A.Length(), Tolerance);
                Assert.Equal(1.0, children[i].B.Length(), Tolerance);
                Assert.Equal(1.0, children[i].C.Length(), Tolerance);
            }
        }

        [Fact]
        public void Subdivide_Order_FirstChildIsApex()
        {
            // Child 0 should include vertex A of the parent
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();

            Assert.True(children[0].Contains(tri.A),
                "Child 0 (apex) should contain parent vertex A");
        }

        [Fact]
        public void Subdivide_Order_SecondChildIsLeft()
        {
            // Child 1 should include vertex B of the parent
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();

            Assert.True(children[1].Contains(tri.B),
                "Child 1 (left) should contain parent vertex B");
        }

        [Fact]
        public void Subdivide_Order_ThirdChildIsRight()
        {
            // Child 2 should include vertex C of the parent
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] children = tri.Subdivide();

            Assert.True(children[2].Contains(tri.C),
                "Child 2 (right) should contain parent vertex C");
        }

        [Fact]
        public void Subdivide_TwoLevels_ProduceSixteenChildren()
        {
            Triangle3D tri = CreateOctantTriangle();
            Triangle3D[] level1 = tri.Subdivide();
            int count = 0;
            double totalArea = 0;
            for (int i = 0; i < level1.Length; i++)
            {
                Triangle3D[] level2 = level1[i].Subdivide();
                count += level2.Length;
                for (int j = 0; j < level2.Length; j++)
                {
                    totalArea += level2[j].Area();
                }
            }
            Assert.Equal(16, count);
            Assert.Equal(tri.Area(), totalArea, AreaTolerance);
        }

        // === Contains ===

        [Fact]
        public void Contains_CentroidIsInside()
        {
            Triangle3D tri = CreateOctantTriangle();
            Assert.True(tri.Contains(tri.Centroid));
        }

        [Fact]
        public void Contains_VerticesAreInside()
        {
            Triangle3D tri = CreateOctantTriangle();
            Assert.True(tri.Contains(tri.A));
            Assert.True(tri.Contains(tri.B));
            Assert.True(tri.Contains(tri.C));
        }

        [Fact]
        public void Contains_OppositePoint_IsFalse()
        {
            Triangle3D tri = CreateOctantTriangle();
            Vector3D opposite = new Vector3D(-1, -1, -1).Normalize();
            Assert.False(tri.Contains(opposite));
        }

        [Fact]
        public void Contains_PointOnOtherHemisphere_IsFalse()
        {
            Triangle3D tri = CreateOctantTriangle();
            Vector3D point = Vector3D.FromLatLon(-45.0, 45.0);
            Assert.False(tri.Contains(point));
        }

        [Fact]
        public void Contains_PointJustOutside_IsFalse()
        {
            Triangle3D tri = CreateOctantTriangle();
            // Point in the adjacent octant (negative X)
            Vector3D point = Vector3D.FromLatLon(45.0, 135.0);
            Assert.False(tri.Contains(point));
        }

        // === GetBarycentricCoordinates ===

        [Fact]
        public void BarycentricCoordinates_CentroidSumsToOne()
        {
            Triangle3D tri = CreateOctantTriangle();
            var (u, v, w) = tri.GetBarycentricCoordinates(tri.Centroid);
            Assert.Equal(1.0, u + v + w, AreaTolerance);
        }

        [Fact]
        public void BarycentricCoordinates_CentroidAllPositive()
        {
            Triangle3D tri = CreateOctantTriangle();
            var (u, v, w) = tri.GetBarycentricCoordinates(tri.Centroid);
            Assert.True(u > 0);
            Assert.True(v > 0);
            Assert.True(w > 0);
        }

        [Fact]
        public void BarycentricCoordinates_VertexA_ReturnsOneZeroZero()
        {
            Triangle3D tri = CreateOctantTriangle();
            var (u, v, w) = tri.GetBarycentricCoordinates(tri.A);
            Assert.Equal(1.0, u, AreaTolerance);
            Assert.Equal(0.0, v, AreaTolerance);
            Assert.Equal(0.0, w, AreaTolerance);
        }

        [Fact]
        public void BarycentricCoordinates_VertexB_ReturnsZeroOneZero()
        {
            Triangle3D tri = CreateOctantTriangle();
            var (u, v, w) = tri.GetBarycentricCoordinates(tri.B);
            Assert.Equal(0.0, u, AreaTolerance);
            Assert.Equal(1.0, v, AreaTolerance);
            Assert.Equal(0.0, w, AreaTolerance);
        }

        [Fact]
        public void BarycentricCoordinates_VertexC_ReturnsZeroZeroOne()
        {
            Triangle3D tri = CreateOctantTriangle();
            var (u, v, w) = tri.GetBarycentricCoordinates(tri.C);
            Assert.Equal(0.0, u, AreaTolerance);
            Assert.Equal(0.0, v, AreaTolerance);
            Assert.Equal(1.0, w, AreaTolerance);
        }

        [Fact]
        public void BarycentricCoordinates_OutsidePointHasNegative()
        {
            Triangle3D tri = CreateOctantTriangle();
            // Point in adjacent octant (negative X side)
            Vector3D outside = Vector3D.FromLatLon(45.0, 135.0);
            var (u, v, w) = tri.GetBarycentricCoordinates(outside);
            Assert.True(u < 0 || v < 0 || w < 0);
        }

        // === Area ===

        [Fact]
        public void Area_OctantTriangle_EqualsHalfPi()
        {
            // One octant of the unit sphere = pi/2 steradians
            Triangle3D tri = CreateOctantTriangle();
            double area = tri.Area();
            Assert.Equal(Math.PI / 2.0, area, AreaTolerance);
        }

        [Fact]
        public void Area_IsAlwaysPositive()
        {
            Triangle3D tri = new Triangle3D(
                Vector3D.FromLatLon(60.0, 25.0),
                Vector3D.FromLatLon(59.0, 24.0),
                Vector3D.FromLatLon(59.5, 26.0));
            Assert.True(tri.Area() > 0);
        }

        [Fact]
        public void Area_EightOctants_CoverFullSphere()
        {
            Vector3D np = new Vector3D(0, 0, 1);
            Vector3D sp = new Vector3D(0, 0, -1);
            Vector3D px = new Vector3D(1, 0, 0);
            Vector3D py = new Vector3D(0, 1, 0);
            Vector3D nx = new Vector3D(-1, 0, 0);
            Vector3D ny = new Vector3D(0, -1, 0);

            double totalArea = 0;
            totalArea += new Triangle3D(np, px, py).Area();
            totalArea += new Triangle3D(np, py, nx).Area();
            totalArea += new Triangle3D(np, nx, ny).Area();
            totalArea += new Triangle3D(np, ny, px).Area();
            totalArea += new Triangle3D(sp, py, px).Area();
            totalArea += new Triangle3D(sp, nx, py).Area();
            totalArea += new Triangle3D(sp, ny, nx).Area();
            totalArea += new Triangle3D(sp, px, ny).Area();

            Assert.Equal(4.0 * Math.PI, totalArea, AreaTolerance);
        }

        // === Equality ===

        [Fact]
        public void Equals_SameVertices_ReturnsTrue()
        {
            Triangle3D a = CreateOctantTriangle();
            Triangle3D b = CreateOctantTriangle();
            Assert.True(a.Equals(b));
            Assert.True(a == b);
        }

        [Fact]
        public void Equals_DifferentVertices_ReturnsFalse()
        {
            Triangle3D a = CreateOctantTriangle();
            Triangle3D b = new Triangle3D(
                new Vector3D(0, 0, -1),
                new Vector3D(1, 0, 0),
                new Vector3D(0, 1, 0));
            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }
    }
}
