namespace Eugene.Linq;

using Enumerators;

public static class EnumerableExtensions
{
  public static IEnumerable<T> FastIntersect<T>(this IEnumerable<T> enumerable1, IEnumerable<T> enumberable2)
    where T : IComparable<T>
  {
    IEnumerator<T> enumerator1 = enumerable1.GetEnumerator();
    IEnumerator<T> enumerator2 = enumberable2.GetEnumerator();

    bool hasValue1 = enumerator1.MoveNext();
    bool hasValue2 = enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      int comparison = enumerator1.Current.CompareTo(enumerator2.Current);

      if (comparison < 0)
      {
        hasValue1 = enumerator1.MoveNext();
      }
      else if (comparison > 0)
      {
        hasValue2 = enumerator2.MoveNext();
      }
      else
      {
        yield return enumerator1.Current;

        hasValue1 = enumerator1.MoveNext();
        hasValue2 = enumerator2.MoveNext();
      }
    }
  }
}
