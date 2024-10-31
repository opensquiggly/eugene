namespace Eugene.Linq;

public class FastIntersectEnumerator<TKey, TData> : IFastIntersectEnumerator<TKey, TData>
  where TKey : IComparable<TKey>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastIntersectEnumerator(
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable1,
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable2
  )
  {
    Enumerator1 = enumerable1.GetFastEnumerator();
    Enumerator2 = enumerable2.GetFastEnumerator();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private IFastEnumerator<TKey, TData> Enumerator1 { get; }

  private IFastEnumerator<TKey, TData> Enumerator2 { get; }

  object IEnumerator.Current => CurrentKey;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TData Current => CurrentData;

  public TKey CurrentKey => Enumerator1.CurrentKey;

  public TData CurrentData => Enumerator1.CurrentData;

  public TData CurrentData1 => Enumerator1.CurrentData;

  public TData CurrentData2 => Enumerator2.CurrentData;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
    Enumerator1.Dispose();
    Enumerator2.Dispose();
  }

  public bool MoveNext()
  {
    bool hasValue1 = Enumerator1.MoveNext();
    bool hasValue2 = Enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      int comparison = Enumerator1.CurrentKey.CompareTo(Enumerator2.CurrentKey);

      if (comparison < 0)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey);
      }
      else if (comparison > 0)
      {
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey);
      }
      else
      {
        return true;
      }
    }

    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(TKey target)
  {
    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    while (hasValue1 && hasValue2)
    {
      int comparison = Enumerator1.CurrentKey.CompareTo(Enumerator2.CurrentKey);

      if (comparison < 0)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey);
      }
      else if (comparison > 0)
      {
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey);
      }
      else
      {
        return true;
      }
    }

    return false;
  }

  public void Reset()
  {
    Enumerator1.Reset();
    Enumerator2.Reset();
  }
}
