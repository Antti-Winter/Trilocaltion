using Trilocation.Core.Indexing;

namespace Trilocation.Core.Algorithms
{
    /// <summary>
    /// Navigates the triangle hierarchy: parent, children, ancestors, descendants, containment.
    /// </summary>
    internal static class HierarchyNavigator
    {
        /// <summary>Returns the parent TriLocation (one level up).</summary>
        public static TriLocation GetParent(TriLocation location)
        {
            if (location.Resolution == 0)
            {
                throw new InvalidOperationException(
                    "Level 0 triangles (base faces) have no parent");
            }
            ulong parentIndex = CumulativeIndex.GetParent(location.Index);
            return new TriLocation(parentIndex);
        }

        /// <summary>Returns the 4 child TriLocations (one level down).</summary>
        public static TriLocation[] GetChildren(TriLocation location)
        {
            if (location.Resolution >= IndexConstants.MaxResolution)
            {
                throw new InvalidOperationException(
                    "Level " + location.Resolution + " triangles have no children (max resolution)");
            }
            ulong[] childIndices = CumulativeIndex.GetChildren(location.Index);
            TriLocation[] children = new TriLocation[4];
            for (int i = 0; i < 4; i++)
            {
                children[i] = new TriLocation(childIndices[i]);
            }
            return children;
        }

        /// <summary>Returns the ancestor at the specified resolution level.</summary>
        public static TriLocation GetAncestor(TriLocation location, int targetResolution)
        {
            if (targetResolution < 0 || targetResolution > location.Resolution)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(targetResolution),
                    "Target resolution must be between 0 and " + location.Resolution);
            }
            if (targetResolution == location.Resolution)
            {
                return location;
            }
            ulong current = location.Index;
            for (int level = location.Resolution; level > targetResolution; level--)
            {
                current = CumulativeIndex.GetParent(current);
            }
            return new TriLocation(current);
        }

        /// <summary>Returns all descendants down to the specified depth.</summary>
        public static TriLocation[] GetDescendants(TriLocation location, int depth)
        {
            if (depth <= 0)
            {
                return Array.Empty<TriLocation>();
            }
            var result = new List<TriLocation>();
            var currentLevel = new List<TriLocation> { location };
            for (int d = 0; d < depth; d++)
            {
                var nextLevel = new List<TriLocation>();
                for (int i = 0; i < currentLevel.Count; i++)
                {
                    if (currentLevel[i].Resolution >= IndexConstants.MaxResolution)
                    {
                        continue;
                    }
                    ulong[] childIndices = CumulativeIndex.GetChildren(currentLevel[i].Index);
                    for (int c = 0; c < 4; c++)
                    {
                        TriLocation child = new TriLocation(childIndices[c]);
                        nextLevel.Add(child);
                        result.Add(child);
                    }
                }
                currentLevel = nextLevel;
            }
            return result.ToArray();
        }

        /// <summary>Checks whether parent contains child in the hierarchy.</summary>
        public static bool Contains(TriLocation parent, TriLocation child)
        {
            if (child.Resolution < parent.Resolution)
            {
                return false;
            }
            if (parent.BaseFace != child.BaseFace)
            {
                return false;
            }
            TriLocation ancestor = GetAncestor(child, parent.Resolution);
            return ancestor.Index == parent.Index;
        }

        /// <summary>Returns the resolution level of the common ancestor.</summary>
        public static int GetCommonAncestorLevel(TriLocation a, TriLocation b)
        {
            if (a.BaseFace != b.BaseFace)
            {
                return -1;
            }
            ulong indexA = a.Index;
            ulong indexB = b.Index;
            int levelA = a.Resolution;
            int levelB = b.Resolution;

            while (levelA > levelB)
            {
                indexA = CumulativeIndex.GetParent(indexA);
                levelA--;
            }
            while (levelB > levelA)
            {
                indexB = CumulativeIndex.GetParent(indexB);
                levelB--;
            }
            while (indexA != indexB)
            {
                indexA = CumulativeIndex.GetParent(indexA);
                indexB = CumulativeIndex.GetParent(indexB);
                levelA--;
            }
            return levelA;
        }
    }
}
