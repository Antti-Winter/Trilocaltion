using Trilocation.Core.Primitives;

namespace Trilocation.Core.Geometry
{
    /// <summary>
    /// Octahedral projection: maps the sphere to 8 base triangular faces.
    /// Vertices: North pole (0,0,1), South pole (0,0,-1),
    /// and 4 equatorial points: (1,0,0), (0,1,0), (-1,0,0), (0,-1,0).
    /// </summary>
    internal static class OctahedralProjection
    {
        /// <summary>North pole vertex.</summary>
        public static readonly Vector3D NorthPole = new Vector3D(0, 0, 1);

        /// <summary>South pole vertex.</summary>
        public static readonly Vector3D SouthPole = new Vector3D(0, 0, -1);

        /// <summary>Equatorial vertex at 0 degrees longitude.</summary>
        public static readonly Vector3D Eq0 = new Vector3D(1, 0, 0);

        /// <summary>Equatorial vertex at 90 degrees longitude.</summary>
        public static readonly Vector3D Eq90 = new Vector3D(0, 1, 0);

        /// <summary>Equatorial vertex at 180 degrees longitude.</summary>
        public static readonly Vector3D Eq180 = new Vector3D(-1, 0, 0);

        /// <summary>Equatorial vertex at 270 degrees longitude.</summary>
        public static readonly Vector3D Eq270 = new Vector3D(0, -1, 0);

        private static readonly Vector3D[] EquatorialVertices = new Vector3D[]
        {
            Eq0, Eq90, Eq180, Eq270
        };

        private static readonly Triangle3D[] BaseFacesCache;

        static OctahedralProjection()
        {
            BaseFacesCache = BuildBaseFaces();
        }

        /// <summary>
        /// Returns the 8 base faces of the octahedron.
        /// Faces 0-3: northern hemisphere, Faces 4-7: southern hemisphere.
        /// Northern faces: NP + Eq[i] + Eq[i+1] (counterclockwise from outside).
        /// Southern faces: SP + Eq[i+1] + Eq[i] (counterclockwise from outside).
        /// </summary>
        public static Triangle3D[] GetBaseFaces()
        {
            return BaseFacesCache;
        }

        /// <summary>
        /// Determines which base face a lat/lon point belongs to.
        /// Returns the face index (0-7).
        /// </summary>
        public static int LatLonToFace(double latitude, double longitude)
        {
            Vector3D point = Vector3D.FromLatLon(latitude, longitude);
            return PointToFace(point);
        }

        /// <summary>
        /// Determines which base face a unit sphere point belongs to.
        /// Returns the face index (0-7).
        /// </summary>
        public static int PointToFace(Vector3D point)
        {
            // Determine longitude quadrant (0-3)
            int quadrant = GetLongitudeQuadrant(point);

            // Northern or southern hemisphere
            if (point.Z >= 0)
            {
                return quadrant;
            }
            else
            {
                return 4 + quadrant;
            }
        }

        private static Triangle3D[] BuildBaseFaces()
        {
            Triangle3D[] faces = new Triangle3D[8];

            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;

                // Northern face i: NorthPole, Eq[i], Eq[next] — counterclockwise from outside
                faces[i] = new Triangle3D(NorthPole, EquatorialVertices[i], EquatorialVertices[next]);

                // Southern face i+4: SouthPole, Eq[next], Eq[i] — counterclockwise from outside
                faces[i + 4] = new Triangle3D(SouthPole, EquatorialVertices[next], EquatorialVertices[i]);
            }

            return faces;
        }

        private static int GetLongitudeQuadrant(Vector3D point)
        {
            double lon = Math.Atan2(point.Y, point.X);
            if (lon < 0)
            {
                lon += 2.0 * Math.PI;
            }

            // Quadrants: 0=[0,90), 1=[90,180), 2=[180,270), 3=[270,360)
            double quadrantAngle = Math.PI / 2.0;
            int quadrant = (int)(lon / quadrantAngle);

            if (quadrant >= 4)
            {
                quadrant = 3;
            }

            return quadrant;
        }
    }
}
