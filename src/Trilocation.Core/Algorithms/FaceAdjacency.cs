namespace Trilocation.Core.Algorithms
{
    /// <summary>
    /// Lookup table for octahedral base face adjacency.
    /// Each face (0-7) has exactly 3 edge-neighbors.
    /// Northern faces 0-3: (NP, Eq[i], Eq[i+1]).
    /// Southern faces 4-7: (SP, Eq[i+1], Eq[i]).
    /// Edge 0 = A-B, Edge 1 = B-C, Edge 2 = C-A.
    /// </summary>
    internal static class FaceAdjacency
    {
        // NeighborTable[face][edge] = neighbor face index
        private static readonly int[][] NeighborTable = new int[][]
        {
            // Face 0 (NP, Eq0, Eq90): edge0->F3, edge1->F4, edge2->F1
            new int[] { 3, 4, 1 },
            // Face 1 (NP, Eq90, Eq180): edge0->F0, edge1->F5, edge2->F2
            new int[] { 0, 5, 2 },
            // Face 2 (NP, Eq180, Eq270): edge0->F1, edge1->F6, edge2->F3
            new int[] { 1, 6, 3 },
            // Face 3 (NP, Eq270, Eq0): edge0->F2, edge1->F7, edge2->F0
            new int[] { 2, 7, 0 },
            // Face 4 (SP, Eq90, Eq0): edge0->F5, edge1->F0, edge2->F7
            new int[] { 5, 0, 7 },
            // Face 5 (SP, Eq180, Eq90): edge0->F6, edge1->F1, edge2->F4
            new int[] { 6, 1, 4 },
            // Face 6 (SP, Eq270, Eq180): edge0->F7, edge1->F2, edge2->F5
            new int[] { 7, 2, 5 },
            // Face 7 (SP, Eq0, Eq270): edge0->F4, edge1->F3, edge2->F6
            new int[] { 4, 3, 6 }
        };

        // EdgeTable[face][edge] = neighbor's edge index for the shared edge
        // Pattern: edge 0 <-> edge 2, edge 1 <-> edge 1
        private static readonly int[][] EdgeTable = new int[][]
        {
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 },
            new int[] { 2, 1, 0 }
        };

        /// <summary>
        /// Returns the neighbor face index across the given edge.
        /// Edge 0: A-B, Edge 1: B-C, Edge 2: C-A.
        /// </summary>
        public static int GetNeighborFace(int face, int edgeIndex)
        {
            return NeighborTable[face][edgeIndex];
        }

        /// <summary>
        /// Returns the edge index on the neighbor face that corresponds to the shared edge.
        /// </summary>
        public static int GetNeighborEdge(int face, int edgeIndex)
        {
            return EdgeTable[face][edgeIndex];
        }

        /// <summary>
        /// Returns all 3 neighbor face indices for the given face.
        /// </summary>
        public static int[] GetAllNeighborFaces(int face)
        {
            return new int[]
            {
                NeighborTable[face][0],
                NeighborTable[face][1],
                NeighborTable[face][2]
            };
        }
    }
}
