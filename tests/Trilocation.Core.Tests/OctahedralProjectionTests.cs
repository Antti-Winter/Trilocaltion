using Trilocation.Core.Geometry;
using Trilocation.Core.Primitives;
using Xunit;

namespace Trilocation.Core.Tests
{
    public class OctahedralProjectionTests
    {
        private const double Tolerance = 1e-10;
        private const double AreaTolerance = 1e-6;

        // === GetBaseFaces ===

        [Fact]
        public void GetBaseFaces_ReturnsEightFaces()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            Assert.Equal(8, faces.Length);
        }

        [Fact]
        public void GetBaseFaces_AllVerticesNormalized()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            for (int i = 0; i < faces.Length; i++)
            {
                Assert.Equal(1.0, faces[i].A.Length(), Tolerance);
                Assert.Equal(1.0, faces[i].B.Length(), Tolerance);
                Assert.Equal(1.0, faces[i].C.Length(), Tolerance);
            }
        }

        [Fact]
        public void GetBaseFaces_TotalAreaCoversFullSphere()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            double totalArea = 0;
            for (int i = 0; i < faces.Length; i++)
            {
                totalArea += faces[i].Area();
            }
            Assert.Equal(4.0 * Math.PI, totalArea, AreaTolerance);
        }

        [Fact]
        public void GetBaseFaces_EachFaceHasArea_HalfPi()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            for (int i = 0; i < faces.Length; i++)
            {
                Assert.Equal(Math.PI / 2.0, faces[i].Area(), AreaTolerance);
            }
        }

        [Fact]
        public void GetBaseFaces_FirstFourAreNorthern()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            for (int i = 0; i < 4; i++)
            {
                Assert.True(faces[i].Centroid.Z > 0,
                    "Face " + i + " centroid should be in northern hemisphere");
            }
        }

        [Fact]
        public void GetBaseFaces_LastFourAreSouthern()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            for (int i = 4; i < 8; i++)
            {
                Assert.True(faces[i].Centroid.Z < 0,
                    "Face " + i + " centroid should be in southern hemisphere");
            }
        }

        // === LatLonToFace ===

        [Fact]
        public void LatLonToFace_NorthPole_ReturnsNorthernFace()
        {
            int face = OctahedralProjection.LatLonToFace(89.99, 45.0);
            Assert.InRange(face, 0, 3);
        }

        [Fact]
        public void LatLonToFace_SouthPole_ReturnsSouthernFace()
        {
            int face = OctahedralProjection.LatLonToFace(-89.99, 45.0);
            Assert.InRange(face, 4, 7);
        }

        [Fact]
        public void LatLonToFace_Helsinki_ReturnsValidFace()
        {
            // Helsinki: 60.17N, 24.94E (northern hemisphere, 0-90 longitude quadrant)
            int face = OctahedralProjection.LatLonToFace(60.17, 24.94);
            Assert.InRange(face, 0, 7);
            // Should be a northern face
            Assert.InRange(face, 0, 3);
        }

        [Fact]
        public void LatLonToFace_Sydney_ReturnsSouthernFace()
        {
            // Sydney: 33.87S, 151.21E (southern hemisphere)
            int face = OctahedralProjection.LatLonToFace(-33.87, 151.21);
            Assert.InRange(face, 4, 7);
        }

        [Fact]
        public void LatLonToFace_EveryPointMapsToExactlyOneFace()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            // Test a grid of points
            double[] lats = new double[] { -80, -45, -10, 0, 10, 45, 80 };
            double[] lons = new double[] { -170, -90, -45, 0, 45, 90, 135, 170 };

            for (int li = 0; li < lats.Length; li++)
            {
                for (int lo = 0; lo < lons.Length; lo++)
                {
                    int face = OctahedralProjection.LatLonToFace(lats[li], lons[lo]);
                    Assert.InRange(face, 0, 7);
                }
            }
        }

        [Fact]
        public void LatLonToFace_PointIsInsideAssignedFace()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            double[] lats = new double[] { -60, -30, 30, 60 };
            double[] lons = new double[] { -135, -45, 45, 135 };

            for (int li = 0; li < lats.Length; li++)
            {
                for (int lo = 0; lo < lons.Length; lo++)
                {
                    int face = OctahedralProjection.LatLonToFace(lats[li], lons[lo]);
                    Vector3D point = Vector3D.FromLatLon(lats[li], lons[lo]);
                    Assert.True(faces[face].Contains(point),
                        "Point (" + lats[li] + ", " + lons[lo] + ") should be in face " + face);
                }
            }
        }

        // === PointToFace ===

        [Fact]
        public void PointToFace_MatchesLatLonToFace()
        {
            double[] lats = new double[] { -60, -30, 30, 60 };
            double[] lons = new double[] { -135, -45, 45, 135 };

            for (int li = 0; li < lats.Length; li++)
            {
                for (int lo = 0; lo < lons.Length; lo++)
                {
                    int faceFromLatLon = OctahedralProjection.LatLonToFace(lats[li], lons[lo]);
                    Vector3D point = Vector3D.FromLatLon(lats[li], lons[lo]);
                    int faceFromPoint = OctahedralProjection.PointToFace(point);
                    Assert.Equal(faceFromLatLon, faceFromPoint);
                }
            }
        }

        // === Consistency with CumulativeIndex base face ===

        [Fact]
        public void GetBaseFaces_ConsistentWithBaseFaceCount()
        {
            Triangle3D[] faces = OctahedralProjection.GetBaseFaces();
            Assert.Equal(Trilocation.Core.Indexing.IndexConstants.BaseFaceCount, faces.Length);
        }
    }
}
