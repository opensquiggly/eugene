namespace Eugene.Collections;

public class DiskSortedArray<TData> : DiskArray<TData> where TData : struct, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedArray(DiskSortedArrayFactory<TData> factory, long address) : base(factory, address)
  {
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  public override int AddItem(TData item)
  {
    EnsureLoaded();
    
    int index = FindLastLessThanOrEqual(item) + 1;
    Grow(1);
    ShiftRight(index);
    this[index] = item;

    return index;
  }

  public int FindLastLessThan(TData data)
  {
    // Find the last element that is less than the 'data' value
    EnsureLoaded();
    
    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) < 0)
      {
        result = mid;
        start = mid + 1;
      }
      else
      {
        end = mid - 1;
      }
    }

    return result;
  }

  public int FindLastLessThanOrEqual(TData data)
  {
    // Find the last element that is less than or equal to the 'data' value 
    EnsureLoaded();

    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) <= 0)
      {
        result = mid;
        start = mid + 1;
      }
      else
      {
        end = mid - 1;
      }
    }

    return result;
  }

  public int FindFirstEqual(TData data)
  {
    // Find the first element that is equal to the the 'data' value
    EnsureLoaded();

    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) == 0)
      {
        result = mid;
        end = mid - 1;
      }
      else if (this[mid].CompareTo(data) < 0)
      {
        start = mid + 1;
      }
      else
      {
        end = mid - 1;
      }
    }

    return result;
  }
  
  public int FindLastEqual(TData data)
  {
    // Find the last element that is equal to the the 'data' value
    EnsureLoaded();

    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) == 0)
      {
        result = mid;
        start = mid + 1;
      }
      else if (this[mid].CompareTo(data) < 0)
      {
        start = mid + 1;
      }
      else
      {
        end = mid - 1;
      }
    }

    return result;
  }

  public int FindFirstGreaterThan(TData data)
  {
    // Find the first element that is equal to the 'data' value
    EnsureLoaded();

    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) > 0)
      {
        result = mid;
        end = mid - 1;
      }
      else
      {
        start = mid + 1;
      }
    }

    return result;
  }

  public int FindFirstGreaterThanOrEqual(TData data)
  {
    // Find the first element that is greater than or equal to the 'data' value
    EnsureLoaded();

    int start = 0;
    int end = this.Count - 1;
    int result = -1;
    
    while (start <= end)
    {
      int mid = (start + end) / 2;
      if (this[mid].CompareTo(data) >= 0)
      {
        result = mid;
        end = mid - 1;
      }
      else
      {
        start = mid + 1;
      }
    }

    return result;
  }
}
