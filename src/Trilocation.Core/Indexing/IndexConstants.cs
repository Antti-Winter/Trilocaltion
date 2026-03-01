namespace Trilocation.Core.Indexing
{
    /// <summary>
    /// Constants and pre-computed lookup tables for cumulative indexing.
    /// </summary>
    public static class IndexConstants
    {
        /// <summary>Maximum resolution level (ulong overflow boundary).</summary>
        public const int MaxResolution = 30;

        /// <summary>Number of triangles at level 0 (8 base faces).</summary>
        public const int BaseFaceCount = 8;

        /// <summary>Number of child triangles per triangle (1:4 quaternary subdivision).</summary>
        public const int ChildrenPerTriangle = 4;

        /// <summary>Pre-computed level start indices. LevelStartTable[n] = S(n-1)+1 or 1 when n=0.</summary>
        public static readonly ulong[] LevelStartTable;

        /// <summary>Pre-computed level end indices. LevelEndTable[n] = S(n).</summary>
        public static readonly ulong[] LevelEndTable;

        static IndexConstants()
        {
            LevelStartTable = new ulong[MaxResolution + 1];
            LevelEndTable = new ulong[MaxResolution + 1];

            for (int i = 0; i <= MaxResolution; i++)
            {
                LevelEndTable[i] = CumulativeIndex.CumulativeCount(i);
                LevelStartTable[i] = i > 0 ? LevelEndTable[i - 1] + 1 : 1;
            }
        }
    }
}
