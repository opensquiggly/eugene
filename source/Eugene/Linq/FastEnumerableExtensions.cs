namespace Eugene.Linq;

public static class FastEnumerableExtensions
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Extension Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public static FastIntersectEnumerable<TKey, TData> FastIntersect<TKey, TData>(
    this IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable1,
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable2)
    where TKey : IComparable<TKey>
  {
    return new FastIntersectEnumerable<TKey, TData>(enumerable1, enumerable2);
  }

  public static IFastUnionEnumerable<TKey, TData> FastUnion<TKey, TData>(
    this IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable1,
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable2)
    where TKey : IComparable<TKey>
  {
    return new FastUnionEnumerable<TKey, TData>(enumerable1, enumerable2);
  }
}
