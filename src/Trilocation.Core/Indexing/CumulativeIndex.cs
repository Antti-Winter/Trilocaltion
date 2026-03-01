using System.Runtime.CompilerServices;

namespace Trilocation.Core.Indexing
{
    /// <summary>
    /// Cumulative index arithmetic.
    /// </summary>
    public static class CumulativeIndex
    {
        /// <summary>
        /// Cumulative sum: S(n) = 8 * (4^(n+1) - 1) / 3.
        /// Overflow-safe: divides by 3 first, then multiplies by 8.
        /// Works because (4^k - 1) is always divisible by 3 (4 = 1 mod 3).
        /// </summary>
        public static ulong CumulativeCount(int resolution)
        {
            if (resolution < 0 || resolution > IndexConstants.MaxResolution)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resolution),
                    "Resolution must be between 0 and " + IndexConstants.MaxResolution);
            }
            return ((1UL << (2 * (resolution + 1))) - 1) / 3 * 8UL;
        }

        /// <summary>First index of the level. Uses pre-computed lookup table.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LevelStart(int resolution)
        {
            if (resolution < 0 || resolution > IndexConstants.MaxResolution)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution));
            }
            return IndexConstants.LevelStartTable[resolution];
        }

        /// <summary>Last index of the level. Uses pre-computed lookup table.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong LevelEnd(int resolution)
        {
            if (resolution < 0 || resolution > IndexConstants.MaxResolution)
            {
                throw new ArgumentOutOfRangeException(nameof(resolution));
            }
            return IndexConstants.LevelEndTable[resolution];
        }

        /// <summary>
        /// Determines the resolution level from the index magnitude.
        /// Linear search, max 31 iterations, O(1).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetResolution(ulong index)
        {
            if (index == 0)
            {
                throw new ArgumentException("Index must be >= 1", nameof(index));
            }
            for (int i = 0; i <= IndexConstants.MaxResolution; i++)
            {
                if (index <= IndexConstants.LevelEndTable[i])
                {
                    return i;
                }
            }
            throw new ArgumentOutOfRangeException(
                nameof(index),
                "Index " + index + " exceeds maximum valid index");
        }

        /// <summary>Returns the parent triangle index.</summary>
        public static ulong GetParent(ulong index)
        {
            int resolution = GetResolution(index);
            if (resolution == 0)
            {
                throw new InvalidOperationException(
                    "Level 0 triangles (base faces) have no parent");
            }
            ulong levelStart = IndexConstants.LevelStartTable[resolution];
            ulong positionInLevel = index - levelStart;
            ulong parentLevelStart = IndexConstants.LevelStartTable[resolution - 1];
            ulong parentPosition = positionInLevel / 4;
            return parentLevelStart + parentPosition;
        }

        /// <summary>Returns the 4 child triangle indices.</summary>
        public static ulong[] GetChildren(ulong index)
        {
            int resolution = GetResolution(index);
            if (resolution >= IndexConstants.MaxResolution)
            {
                throw new InvalidOperationException(
                    "Level " + resolution + " triangles have no children (max resolution)");
            }
            ulong levelStart = IndexConstants.LevelStartTable[resolution];
            ulong positionInLevel = index - levelStart;
            ulong childLevelStart = IndexConstants.LevelStartTable[resolution + 1];
            ulong firstChild = childLevelStart + positionInLevel * 4;
            return new ulong[]
            {
                firstChild,
                firstChild + 1,
                firstChild + 2,
                firstChild + 3
            };
        }

        /// <summary>
        /// Returns the base face number (0-7).
        /// Optimized: navigates to the root with a single loop.
        /// </summary>
        public static int GetBaseFace(ulong index)
        {
            int resolution = GetResolution(index);
            ulong current = index;
            for (int level = resolution; level >= 1; level--)
            {
                ulong levelStart = IndexConstants.LevelStartTable[level];
                ulong pos = current - levelStart;
                current = IndexConstants.LevelStartTable[level - 1] + pos / 4;
            }
            return (int)(current - 1);
        }
    }
}
