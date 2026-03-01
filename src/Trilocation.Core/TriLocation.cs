using Trilocation.Core.Algorithms;
using Trilocation.Core.Indexing;

namespace Trilocation.Core
{
    /// <summary>
    /// Represents a single triangle location in the hierarchical geospatial index.
    /// </summary>
    public readonly struct TriLocation : IEquatable<TriLocation>, IComparable<TriLocation>
    {
        /// <summary>Cumulative index (1-based).</summary>
        public ulong Index { get; }

        /// <summary>Resolution level (0-30), derived from the index magnitude.</summary>
        public int Resolution { get; }

        /// <summary>Base face (0-7), derived from the index hierarchy.</summary>
        public int BaseFace { get; }

        /// <summary>Creates a TriLocation from a cumulative index.</summary>
        public TriLocation(ulong index)
        {
            IndexValidator.ValidateIndex(index);
            Index = index;
            Resolution = CumulativeIndex.GetResolution(index);
            BaseFace = CumulativeIndex.GetBaseFace(index);
        }

        /// <summary>Creates a TriLocation from lat/lon coordinates at the given resolution.</summary>
        public TriLocation(double latitude, double longitude, int resolution)
        {
            ulong index = CoordinateConverter.ToIndex(latitude, longitude, resolution);
            Index = index;
            Resolution = resolution;
            BaseFace = CumulativeIndex.GetBaseFace(index);
        }

        /// <summary>Converts this TriLocation to lat/lon coordinates (centroid of the triangle).</summary>
        public (double Latitude, double Longitude) ToLatLon()
        {
            return CoordinateConverter.ToLatLon(Index);
        }

        /// <summary>Converts this TriLocation to a TriCell with full geometry.</summary>
        public TriCell ToCell()
        {
            var triangle = CoordinateConverter.GetTriangle(Index);
            return new TriCell(this, triangle);
        }

        // === Hierarchy methods ===

        /// <summary>Returns the parent TriLocation (one level up).</summary>
        public TriLocation GetParent()
        {
            return HierarchyNavigator.GetParent(this);
        }

        /// <summary>Returns the 4 child TriLocations (one level down).</summary>
        public TriLocation[] GetChildren()
        {
            return HierarchyNavigator.GetChildren(this);
        }

        /// <summary>Returns the ancestor at the specified resolution level.</summary>
        public TriLocation GetAncestor(int targetResolution)
        {
            return HierarchyNavigator.GetAncestor(this, targetResolution);
        }

        /// <summary>Checks whether this location hierarchically contains the other.</summary>
        public bool Contains(TriLocation other)
        {
            return HierarchyNavigator.Contains(this, other);
        }

        /// <summary>Returns the resolution level of the common ancestor with another location.</summary>
        public int GetCommonAncestorLevel(TriLocation other)
        {
            return HierarchyNavigator.GetCommonAncestorLevel(this, other);
        }

        // === Neighbor methods ===

        /// <summary>Returns the 3 edge-neighbors of this triangle.</summary>
        public TriLocation[] GetNeighbors()
        {
            return NeighborFinder.GetNeighbors(this);
        }

        /// <summary>Returns all triangles within the given grid-distance.</summary>
        public TriLocation[] GetNeighborsWithin(int distance)
        {
            return NeighborFinder.GetNeighborsWithin(this, distance);
        }

        /// <summary>Equality comparison.</summary>
        public bool Equals(TriLocation other)
        {
            return Index == other.Index;
        }

        /// <summary>Equality comparison.</summary>
        public override bool Equals(object? obj)
        {
            return obj is TriLocation other && Equals(other);
        }

        /// <summary>Returns the hash code.</summary>
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        /// <summary>Compares by index value.</summary>
        public int CompareTo(TriLocation other)
        {
            return Index.CompareTo(other.Index);
        }

        /// <summary>Returns the string representation.</summary>
        public override string ToString()
        {
            return "TriLocation(" + Index + ", R" + Resolution + ", F" + BaseFace + ")";
        }

        /// <summary>Equality operator.</summary>
        public static bool operator ==(TriLocation left, TriLocation right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        public static bool operator !=(TriLocation left, TriLocation right) => !left.Equals(right);

        /// <summary>Less than operator.</summary>
        public static bool operator <(TriLocation left, TriLocation right) => left.Index < right.Index;

        /// <summary>Greater than operator.</summary>
        public static bool operator >(TriLocation left, TriLocation right) => left.Index > right.Index;

        /// <summary>Less than or equal operator.</summary>
        public static bool operator <=(TriLocation left, TriLocation right) => left.Index <= right.Index;

        /// <summary>Greater than or equal operator.</summary>
        public static bool operator >=(TriLocation left, TriLocation right) => left.Index >= right.Index;
    }
}
