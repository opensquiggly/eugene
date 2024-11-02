namespace Eugene.Linq;

public class FastUnionEnumerable<TKey, TData> : IFastUnionEnumerable<TKey, TData>
  where TKey : IComparable<TKey>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastUnionEnumerable(
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable1,
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable2,
    Func<TKey, TData, TKey, TData, int> comparer = null
  )
  {
    Enumerable1 = enumerable1;
    Enumerable2 = enumerable2;
    Comparer = comparer;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> Enumerable1 { get; }

  private IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> Enumerable2 { get; }

  private Func<TKey, TData, TKey, TData, int> Comparer { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IFastUnionEnumerator<TKey, TData> GetFastEnumerator()
  {
    return new FastUnionEnumerator<TKey, TData>(Enumerable1, Enumerable2, Comparer);
  }

  public IEnumerator<TData> GetEnumerator()
  {
    return new FastUnionEnumerator<TKey, TData>(Enumerable1, Enumerable2, Comparer);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
