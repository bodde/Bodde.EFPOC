namespace Bodde.EFPOC.Tests
{
    internal static class EnumerableExtensions
    {
        public static string ToCsv<T>(this IEnumerable<T> source)
        {
            if (source == null || !source.Any())
            {
                return string.Empty;
            }
            return string.Join(",", source.Select(item => item?.ToString() ?? string.Empty));
        }
    }
}
