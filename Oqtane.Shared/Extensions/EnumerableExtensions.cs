namespace System.Collections
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty(this IEnumerable source)
            => source == null || source.GetEnumerator().MoveNext() == false;
    }
}
