using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Trilocation.Data
{
    /// <summary>
    /// EF Core value converter: ulong (C#) to long (database BIGINT).
    /// Uses unchecked cast to preserve bit pattern. Lossless for all values.
    /// </summary>
    public class TriIndexValueConverter : ValueConverter<ulong, long>
    {
        /// <summary>Creates a new TriIndexValueConverter.</summary>
        public TriIndexValueConverter()
            : base(v => unchecked((long)v),
                   v => unchecked((ulong)v))
        {
        }
    }
}
