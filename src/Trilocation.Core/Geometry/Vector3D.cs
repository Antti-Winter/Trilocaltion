using System.Runtime.CompilerServices;
using Trilocation.Core.Primitives;

namespace Trilocation.Core.Geometry
{
    /// <summary>
    /// 3D unit vector for spherical geometry.
    /// </summary>
    public readonly struct Vector3D : IEquatable<Vector3D>
    {
        /// <summary>X component.</summary>
        public double X { get; }

        /// <summary>Y component.</summary>
        public double Y { get; }

        /// <summary>Z component.</summary>
        public double Z { get; }

        /// <summary>Creates a new Vector3D.</summary>
        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>Normalizes the vector to a unit vector.</summary>
        public Vector3D Normalize()
        {
            double len = Length();
            return new Vector3D(X / len, Y / len, Z / len);
        }

        /// <summary>Dot product.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Dot(Vector3D other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>Cross product.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3D Cross(Vector3D other)
        {
            return new Vector3D(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);
        }

        /// <summary>Length of the vector.</summary>
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        /// <summary>Converts lat/lon coordinates to a 3D unit vector.</summary>
        public static Vector3D FromLatLon(double latitude, double longitude)
        {
            double latRad = latitude * GeoConstants.DegreesToRadians;
            double lonRad = longitude * GeoConstants.DegreesToRadians;
            double cosLat = Math.Cos(latRad);
            return new Vector3D(
                cosLat * Math.Cos(lonRad),
                cosLat * Math.Sin(lonRad),
                Math.Sin(latRad));
        }

        /// <summary>Converts the 3D unit vector to lat/lon coordinates.</summary>
        public (double Latitude, double Longitude) ToLatLon()
        {
            double lat = Math.Asin(Z) * GeoConstants.RadiansToDegrees;
            double lon = Math.Atan2(Y, X) * GeoConstants.RadiansToDegrees;
            return (lat, lon);
        }

        /// <summary>Addition operator.</summary>
        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>Subtraction operator.</summary>
        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>Scalar multiplication.</summary>
        public static Vector3D operator *(Vector3D v, double scalar)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        /// <summary>Scalar multiplication.</summary>
        public static Vector3D operator *(double scalar, Vector3D v)
        {
            return new Vector3D(v.X * scalar, v.Y * scalar, v.Z * scalar);
        }

        /// <summary>Equality comparison.</summary>
        public bool Equals(Vector3D other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        /// <summary>Equality comparison.</summary>
        public override bool Equals(object? obj)
        {
            return obj is Vector3D other && Equals(other);
        }

        /// <summary>Returns the hash code.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z);
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(Vector3D left, Vector3D right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(Vector3D left, Vector3D right) => !left.Equals(right);

        /// <summary>Returns the string representation.</summary>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }
    }
}
