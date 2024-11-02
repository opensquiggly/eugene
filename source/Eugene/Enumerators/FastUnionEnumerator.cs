namespace Eugene.Linq;

public class FastUnionEnumerator<TKey, TData> : IFastUnionEnumerator<TKey, TData>
  where TKey : IComparable<TKey>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastUnionEnumerator(
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable1,
    IFastEnumerable<IFastEnumerator<TKey, TData>, TKey, TData> enumerable2,
    Func<TKey, TData, TKey, TData, int> comparer = null
  )
  {
    Enumerator1 = enumerable1.GetFastEnumerator();
    Enumerator2 = enumerable2.GetFastEnumerator();
    Comparer = comparer;
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private IFastEnumerator<TKey, TData> Enumerator1 { get; }

  private IFastEnumerator<TKey, TData> Enumerator2 { get; }

  private Func<TKey, TData, TKey, TData, int> Comparer { get; }

  private IFastEnumerator<TKey, TData> CurrentEnumerator { get; set; }

  object IEnumerator.Current => CurrentKey;

  private bool IsReset { get; set; }

  private bool HasNextValue1 { get; set; }

  private bool HasNextValue2 { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TData Current => CurrentData;

  public TKey CurrentKey => CurrentEnumerator.CurrentKey;

  public TData CurrentData => CurrentEnumerator.CurrentData;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private int Compare(TKey key1, TData data1, TKey key2, TData data2)
  {
    return Comparer?.Invoke(key1, data1, key2, data2) ?? key1.CompareTo(key2);
  }

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
    if (IsReset)
    {
      HasNextValue1 = Enumerator1.MoveNext();
      HasNextValue2 = Enumerator2.MoveNext();
      IsReset = false;
    }
    else if (CurrentEnumerator == Enumerator1)
    {
      HasNextValue1 = Enumerator1.MoveNext();
    }
    else if (CurrentEnumerator == Enumerator2)
    {
      HasNextValue2 = Enumerator2.MoveNext();
    }

    if (HasNextValue1 && HasNextValue2)
    {
      int comparison = Compare(
        Enumerator1.CurrentKey,
        Enumerator1.CurrentData,
        Enumerator2.CurrentKey,
        Enumerator2.CurrentData
      );

      if (comparison <= 0)
      {
        CurrentEnumerator = Enumerator1;
      }
      else if (comparison > 0)
      {
        CurrentEnumerator = Enumerator2;
      }

      return true;
    }

    if (HasNextValue1)
    {
      CurrentEnumerator = Enumerator1;
      return true;
    }

    if (HasNextValue2)
    {
      CurrentEnumerator = Enumerator2;
      return true;
    }

    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(TKey target)
  {
    HasNextValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    HasNextValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    if (HasNextValue1 && HasNextValue2)
    {
      int comparison = Compare(
        Enumerator1.CurrentKey,
        Enumerator1.CurrentData,
        Enumerator2.CurrentKey,
        Enumerator2.CurrentData
      );

      if (comparison <= 0)
      {
        CurrentEnumerator = Enumerator1;
      }
      else if (comparison > 0)
      {
        CurrentEnumerator = Enumerator2;
      }

      return true;
    }

    if (HasNextValue1)
    {
      CurrentEnumerator = Enumerator1;
      return true;
    }

    if (HasNextValue2)
    {
      CurrentEnumerator = Enumerator2;
      return true;
    }

    return false;
  }

  public void Reset()
  {
    Enumerator1.Reset();
    Enumerator2.Reset();
    CurrentEnumerator = Enumerator1;
    IsReset = true;
  }
}
