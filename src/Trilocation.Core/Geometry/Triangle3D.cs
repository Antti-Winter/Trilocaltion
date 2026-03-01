using System.Runtime.CompilerServices;

namespace Trilocation.Core.Geometry
{
    /// <summary>
    /// Spherical triangle defined by three vertices on a unit sphere.
    /// </summary>
    internal readonly struct Triangle3D : IEquatable<Triangle3D>
    {
        /// <summary>First vertex.</summary>
        public Vector3D A { get; }

        /// <summary>Second vertex.</summary>
        public Vector3D B { get; }

        /// <summary>Third vertex.</summary>
        public Vector3D C { get; }

        /// <summary>Centroid of the triangle, projected onto the unit sphere.</summary>
        public Vector3D Centroid { get; }

        /// <summary>Creates a new Triangle3D from three vertices.</summary>
        public Triangle3D(Vector3D a, Vector3D b, Vector3D c)
        {
            A = a;
            B = b;
            C = c;
            Centroid = (a + b + c).Normalize();
        }

        /// <summary>
        /// Subdivides the triangle into 4 child triangles (quaternary subdivision).
        /// Child order: 0=apex (top), 1=left, 2=right, 3=center (inverted).
        /// Midpoints are normalized to the unit sphere.
        /// </summary>
        public Triangle3D[] Subdivide()
        {
            Vector3D midAB = SphericalMath.MidpointOnSphere(A, B);
            Vector3D midBC = SphericalMath.MidpointOnSphere(B, C);
            Vector3D midCA = SphericalMath.MidpointOnSphere(C, A);

            return new Triangle3D[]
            {
                new Triangle3D(A, midAB, midCA),       // 0: apex (contains vertex A)
                new Triangle3D(midAB, B, midBC),       // 1: left (contains vertex B)
                new Triangle3D(midCA, midBC, C),       // 2: right (contains vertex C)
                new Triangle3D(midBC, midCA, midAB)    // 3: center (inverted)
            };
        }

        /// <summary>Checks whether a point on the unit sphere lies inside this triangle.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(Vector3D point)
        {
            double d1 = ScalarTriple(A, B, point);
            double d2 = ScalarTriple(B, C, point);
            double d3 = ScalarTriple(C, A, point);

            // Check triangle orientation
            double orient = ScalarTriple(A, B, C);

            if (orient >= 0)
            {
                // Counterclockwise: all triple products must be >= 0
                return d1 >= -1e-14 && d2 >= -1e-14 && d3 >= -1e-14;
            }
            else
            {
                // Clockwise: all triple products must be <= 0
                return d1 <= 1e-14 && d2 <= 1e-14 && d3 <= 1e-14;
            }
        }

        /// <summary>
        /// Returns barycentric coordinates (u, v, w) of a point relative to this triangle.
        /// For a point inside the triangle: u >= 0, v >= 0, w >= 0, u + v + w approx 1.
        /// Uses scalar triple product ratios for sign-preserving coordinates.
        /// </summary>
        public (double U, double V, double W) GetBarycentricCoordinates(Vector3D point)
        {
            double uRaw = ScalarTriple(point, B, C);
            double vRaw = ScalarTriple(A, point, C);
            double wRaw = ScalarTriple(A, B, point);

            double total = uRaw + vRaw + wRaw;

            if (Math.Abs(total) < 1e-20)
            {
                return (1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0);
            }

            return (uRaw / total, vRaw / total, wRaw / total);
        }

        /// <summary>Spherical area of this triangle on a unit sphere (steradians).</summary>
        public double Area()
        {
            return SphericalMath.SphericalExcess(A, B, C);
        }

        /// <summary>
        /// Scalar triple product: a . (b x c).
        /// Equals the determinant of the 3x3 matrix [a; b; c].
        /// </summary>
        private static double ScalarTriple(Vector3D a, Vector3D b, Vector3D c)
        {
            return a.X * (b.Y * c.Z - b.Z * c.Y)
                 + a.Y * (b.Z * c.X - b.X * c.Z)
                 + a.Z * (b.X * c.Y - b.Y * c.X);
        }

        /// <summary>Equality comparison.</summary>
        public bool Equals(Triangle3D other)
        {
            return A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C);
        }

        /// <summary>Equality comparison.</summary>
        public override bool Equals(object? obj)
        {
            return obj is Triangle3D other && Equals(other);
        }

        /// <summary>Returns the hash code.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(A, B, C);
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(Triangle3D left, Triangle3D right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Triangle3D left, Triangle3D right) => !left.Equals(right);
    }
}
