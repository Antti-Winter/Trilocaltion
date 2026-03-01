using Xunit;
using Trilocation.Core.Algorithms;

namespace Trilocation.Core.Tests
{
    public class FaceAdjacencyTests
    {
        // === GetNeighborFace ===

        [Theory]
        [InlineData(0, 0, 3)]  // Face 0, edge A-B (NP-Eq0) -> Face 3
        [InlineData(0, 1, 4)]  // Face 0, edge B-C (Eq0-Eq90) -> Face 4
        [InlineData(0, 2, 1)]  // Face 0, edge C-A (Eq90-NP) -> Face 1
        [InlineData(1, 0, 0)]  // Face 1, edge A-B (NP-Eq90) -> Face 0
        [InlineData(1, 1, 5)]  // Face 1, edge B-C (Eq90-Eq180) -> Face 5
        [InlineData(1, 2, 2)]  // Face 1, edge C-A (Eq180-NP) -> Face 2
        [InlineData(2, 0, 1)]  // Face 2, edge A-B (NP-Eq180) -> Face 1
        [InlineData(2, 1, 6)]  // Face 2, edge B-C (Eq180-Eq270) -> Face 6
        [InlineData(2, 2, 3)]  // Face 2, edge C-A (Eq270-NP) -> Face 3
        [InlineData(3, 0, 2)]  // Face 3, edge A-B (NP-Eq270) -> Face 2
        [InlineData(3, 1, 7)]  // Face 3, edge B-C (Eq270-Eq0) -> Face 7
        [InlineData(3, 2, 0)]  // Face 3, edge C-A (Eq0-NP) -> Face 0
        [InlineData(4, 0, 5)]  // Face 4, edge A-B (SP-Eq90) -> Face 5
        [InlineData(4, 1, 0)]  // Face 4, edge B-C (Eq90-Eq0) -> Face 0
        [InlineData(4, 2, 7)]  // Face 4, edge C-A (Eq0-SP) -> Face 7
        [InlineData(5, 0, 6)]  // Face 5, edge A-B (SP-Eq180) -> Face 6
        [InlineData(5, 1, 1)]  // Face 5, edge B-C (Eq180-Eq90) -> Face 1
        [InlineData(5, 2, 4)]  // Face 5, edge C-A (Eq90-SP) -> Face 4
        [InlineData(6, 0, 7)]  // Face 6, edge A-B (SP-Eq270) -> Face 7
        [InlineData(6, 1, 2)]  // Face 6, edge B-C (Eq270-Eq180) -> Face 6
        [InlineData(6, 2, 5)]  // Face 6, edge C-A (Eq180-SP) -> Face 5
        [InlineData(7, 0, 4)]  // Face 7, edge A-B (SP-Eq0) -> Face 4
        [InlineData(7, 1, 3)]  // Face 7, edge B-C (Eq0-Eq270) -> Face 3
        [InlineData(7, 2, 6)]  // Face 7, edge C-A (Eq270-SP) -> Face 6
        public void GetNeighborFace_KnownAdjacencies(int face, int edgeIndex, int expectedNeighbor)
        {
            int neighbor = FaceAdjacency.GetNeighborFace(face, edgeIndex);
            Assert.Equal(expectedNeighbor, neighbor);
        }

        // === GetNeighborEdge ===

        [Theory]
        [InlineData(0, 0, 2)]  // Face 0 edge 0 <-> Face 3 edge 2
        [InlineData(0, 1, 1)]  // Face 0 edge 1 <-> Face 4 edge 1
        [InlineData(0, 2, 0)]  // Face 0 edge 2 <-> Face 1 edge 0
        [InlineData(4, 0, 2)]  // Face 4 edge 0 <-> Face 5 edge 2
        [InlineData(4, 1, 1)]  // Face 4 edge 1 <-> Face 0 edge 1
        [InlineData(4, 2, 0)]  // Face 4 edge 2 <-> Face 7 edge 0
        public void GetNeighborEdge_KnownValues(int face, int edgeIndex, int expectedNeighborEdge)
        {
            int neighborEdge = FaceAdjacency.GetNeighborEdge(face, edgeIndex);
            Assert.Equal(expectedNeighborEdge, neighborEdge);
        }

        // === Symmetry ===

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void GetNeighborFace_Symmetry_BidirectionalRelationship(int face)
        {
            for (int edge = 0; edge < 3; edge++)
            {
                int neighborFace = FaceAdjacency.GetNeighborFace(face, edge);
                int neighborEdge = FaceAdjacency.GetNeighborEdge(face, edge);

                // Reverse: neighbor's neighbor across the shared edge should be the original face
                int reverseNeighbor = FaceAdjacency.GetNeighborFace(neighborFace, neighborEdge);
                Assert.Equal(face, reverseNeighbor);
            }
        }

        // === GetAllNeighborFaces ===

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        public void GetAllNeighborFaces_ReturnsThreeDistinctNeighbors(int face)
        {
            int[] neighbors = FaceAdjacency.GetAllNeighborFaces(face);

            Assert.Equal(3, neighbors.Length);
            Assert.Equal(neighbors.Length, neighbors.Distinct().Count());
            Assert.DoesNotContain(face, neighbors);
            Assert.All(neighbors, n => Assert.InRange(n, 0, 7));
        }
    }
}
