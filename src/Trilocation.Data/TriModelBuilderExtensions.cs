using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Trilocation.Data
{
    /// <summary>
    /// Extension methods for configuring TriIndex columns in EF Core model builder.
    /// </summary>
    public static class TriModelBuilderExtensions
    {
        /// <summary>
        /// Configures the TriIndex property with value converter, column type, and database index.
        /// Call this in OnModelCreating for each entity that implements IHasTriIndex.
        /// </summary>
        public static EntityTypeBuilder<T> ConfigureTriIndex<T>(
            this EntityTypeBuilder<T> builder) where T : class, IHasTriIndex
        {
            builder.Property(e => e.TriIndex)
                .HasConversion(new TriIndexValueConverter())
                .HasColumnType("BIGINT");

            builder.HasIndex(e => e.TriIndex);

            return builder;
        }
    }
}
