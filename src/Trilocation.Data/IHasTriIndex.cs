namespace Trilocation.Data
{
    /// <summary>
    /// Interface for entities that have a TriIndex spatial column.
    /// Implement this on your entity class to enable spatial queries.
    /// </summary>
    public interface IHasTriIndex
    {
        /// <summary>The cumulative triangle index (1-based, stored as BIGINT).</summary>
        ulong TriIndex { get; set; }
    }
}
