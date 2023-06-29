namespace Eugene.Linq;

using Enumerators;

public static class FastEnumerableExtensions
{
  public static IEnumerable<TData> FastIntersect<TKey, TData>(
    this IFastEnumerable<TKey, TData> enumerable1,
    IFastEnumerable<TKey, TData> enumberable2)
    where TKey : IComparable<TKey>
  {
    IFastEnumerator<TKey, TData> enumerator1 = enumerable1.GetFastEnumerator();
    IFastEnumerator<TKey, TData> enumerator2 = enumberable2.GetFastEnumerator();

    bool hasValue1 = enumerator1.MoveNext();
    bool hasValue2 = enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      int comparison = enumerator1.CurrentKey.CompareTo(enumerator2.CurrentKey);

      if (comparison < 0)
      {
        hasValue1 = enumerator1.MoveUntilGreaterThanOrEqual(enumerator2.CurrentKey);
      }
      else if (comparison > 0)
      {
        hasValue2 = enumerator2.MoveUntilGreaterThanOrEqual(enumerator1.CurrentKey);
      }
      else
      {
        yield return enumerator1.CurrentData;

        hasValue1 = enumerator1.MoveNext();
        hasValue2 = enumerator2.MoveNext();
      }
    }
  }
}
