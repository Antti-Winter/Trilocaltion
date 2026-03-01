namespace Trilocation.Core.Geometry
{
    /// <summary>
    /// Spherical geometry calculations on a unit sphere.
    /// </summary>
    internal static class SphericalMath
    {
        /// <summary>Angular distance between two points on a unit sphere (radians).</summary>
        public static double SphericalDistance(Vector3D a, Vector3D b)
        {
            double dot = a.Dot(b);
            dot = Math.Clamp(dot, -1.0, 1.0);
            return Math.Acos(dot);
        }

        /// <summary>Spherical linear interpolation between two points on a unit sphere.</summary>
        public static Vector3D Slerp(Vector3D a, Vector3D b, double t)
        {
            double dot = a.Dot(b);
            dot = Math.Clamp(dot, -1.0, 1.0);
            double theta = Math.Acos(dot);

            if (theta < 1e-15)
            {
                return a;
            }

            double sinTheta = Math.Sin(theta);
            double factorA = Math.Sin((1.0 - t) * theta) / sinTheta;
            double factorB = Math.Sin(t * theta) / sinTheta;

            return a * factorA + b * factorB;
        }

        /// <summary>Midpoint between two points on a unit sphere (normalized).</summary>
        public static Vector3D MidpointOnSphere(Vector3D a, Vector3D b)
        {
            return (a + b).Normalize();
        }

        /// <summary>
        /// Spherical excess of a spherical triangle (area on unit sphere in steradians).
        /// Uses the Van Oosterom-Strackee formula:
        /// tan(E/2) = |a . (b x c)| / (1 + a.b + a.c + b.c)
        /// </summary>
        public static double SphericalExcess(Vector3D a, Vector3D b, Vector3D c)
        {
            double tripleProduct = a.Dot(b.Cross(c));
            double ab = a.Dot(b);
            double ac = a.Dot(c);
            double bc = b.Dot(c);

            double numerator = Math.Abs(tripleProduct);
            double denominator = 1.0 + ab + ac + bc;

            if (numerator < 1e-15 && Math.Abs(denominator) < 1e-15)
            {
                return 0.0;
            }

            double halfExcess = Math.Atan2(numerator, denominator);
            return 2.0 * Math.Abs(halfExcess);
        }
    }
}
