namespace Trilocation.Core.Indexing
{
    /// <summary>
    /// Index validation.
    /// </summary>
    public static class IndexValidator
    {
        private static readonly ulong MaxIndex = CumulativeIndex.CumulativeCount(IndexConstants.MaxResolution);

        /// <summary>Checks whether the index is valid (1..S(30)).</summary>
        public static bool IsValid(ulong index)
        {
            return index >= 1 && index <= MaxIndex;
        }

        /// <summary>Validates the index and throws an exception if invalid.</summary>
        public static void ValidateIndex(ulong index)
        {
            if (index == 0)
            {
                throw new ArgumentException("Index must be >= 1", nameof(index));
            }
            if (index > MaxIndex)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Index " + index + " exceeds maximum valid index " + MaxIndex);
            }
        }
    }
}
