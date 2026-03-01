using System.Linq.Expressions;
using Trilocation.Core;
using Trilocation.Core.Indexing;

namespace Trilocation.Data
{
    /// <summary>
    /// LINQ query extensions for spatial queries on entities with a TriIndex column.
    /// All queries generate BETWEEN clauses that leverage B-tree database indices.
    /// </summary>
    public static class TriQueryExtensions
    {
        /// <summary>
        /// Filters entities within the area covered by the given ancestor triangle.
        /// Includes the ancestor itself and all descendants at all resolution levels.
        /// Generates OR of BETWEEN clauses, one per resolution level.
        /// </summary>
        public static IQueryable<T> WithinArea<T>(
            this IQueryable<T> query,
            TriLocation ancestor) where T : class, IHasTriIndex
        {
            int resolution = ancestor.Resolution;
            ulong position = ancestor.Index - CumulativeIndex.LevelStart(resolution);

            var ranges = new List<(ulong Start, ulong End)>();
            ranges.Add((ancestor.Index, ancestor.Index));

            ulong power = 1;
            for (int level = resolution + 1; level <= IndexConstants.MaxResolution; level++)
            {
                power *= 4;
                ulong start = CumulativeIndex.LevelStart(level) + position * power;
                ulong end = start + power - 1;
                ranges.Add((start, end));
            }

            return query.Where(BuildOrPredicate<T>(ranges));
        }

        /// <summary>
        /// Filters entities that are direct children (4) of the given parent triangle.
        /// Uses a single BETWEEN on the contiguous child index range.
        /// </summary>
        public static IQueryable<T> ChildrenOf<T>(
            this IQueryable<T> query,
            TriLocation parent) where T : class, IHasTriIndex
        {
            ulong[] children = CumulativeIndex.GetChildren(parent.Index);
            ulong minChild = children[0];
            ulong maxChild = children[3];
            return query.Where(x => x.TriIndex >= minChild && x.TriIndex <= maxChild);
        }

        /// <summary>
        /// Filters entities at a specific resolution level.
        /// Uses a single BETWEEN on the level's index range.
        /// </summary>
        public static IQueryable<T> AtResolution<T>(
            this IQueryable<T> query,
            int resolution) where T : class, IHasTriIndex
        {
            ulong start = CumulativeIndex.LevelStart(resolution);
            ulong end = CumulativeIndex.LevelEnd(resolution);
            return query.Where(x => x.TriIndex >= start && x.TriIndex <= end);
        }

        /// <summary>
        /// Filters entities near the given center by expanding to an ancestor at the specified resolution.
        /// Equivalent to WithinArea(center.GetAncestor(expandResolution)).
        /// </summary>
        public static IQueryable<T> NearBy<T>(
            this IQueryable<T> query,
            TriLocation center,
            int expandResolution) where T : class, IHasTriIndex
        {
            TriLocation area = center.GetAncestor(expandResolution);
            return query.WithinArea(area);
        }

        private static Expression<Func<T, bool>> BuildOrPredicate<T>(
            List<(ulong Start, ulong End)> ranges) where T : class, IHasTriIndex
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, nameof(IHasTriIndex.TriIndex));

            Expression? body = null;

            foreach (var range in ranges)
            {
                Expression condition;
                if (range.Start == range.End)
                {
                    condition = Expression.Equal(
                        property, Expression.Constant(range.Start));
                }
                else
                {
                    var ge = Expression.GreaterThanOrEqual(
                        property, Expression.Constant(range.Start));
                    var le = Expression.LessThanOrEqual(
                        property, Expression.Constant(range.End));
                    condition = Expression.AndAlso(ge, le);
                }

                body = body == null ? condition : Expression.OrElse(body, condition);
            }

            return Expression.Lambda<Func<T, bool>>(body!, parameter);
        }
    }
}
