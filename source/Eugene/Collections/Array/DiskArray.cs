namespace Eugene.Collections;

public class DiskArray<TData> where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskArray(DiskArrayFactory<TData> factory, long address)
  {
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Protected Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  protected ArrayBlock _arrayBlock = default;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Protected Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  protected bool IsLoaded { get; set; } = false;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Indexer
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TData this[int index]
  {
    get
    {
      GetAt(index, out TData temp);
      return temp;
    }
    set => SetAt(index, value);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public int Count
  {
    get
    {
      EnsureLoaded();
      return _arrayBlock.Count;
    }
  }

  public long DataAddress
  {
    get
    {
      EnsureLoaded();
      return _arrayBlock.DataAddress;
    }
  }

  public short DataBlockTypeIndex
  {
    get
    {
      EnsureLoaded();
      return _arrayBlock.DataBlockTypeIndex;
    }
  }

  public int DataSize
  {
    get
    {
      EnsureLoaded();
      return _arrayBlock.DataSize;
    }
  }

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskArrayFactory<TData> Factory { get; }

  public bool IsFull
  {
    get
    {
      EnsureLoaded();
      return this.Count >= _arrayBlock.MaxItems;
    }
  }

  public int MaxItems
  {
    get
    {
      EnsureLoaded();
      return _arrayBlock.MaxItems;
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public virtual int AddItem(TData item)
  {
    // Note: DiskSortedArray overrides this method and adds the item in sorted order
    return AppendItem(item, true);
  }

  public void AddItemsTo(DiskArray<TData> dest, int startIndex, int endIndex = -1)
  {
    // Add items from current array to the destination array starting at
    // startIndex up to BUT NOT INCLUDING end index.
    //
    // If end index is omitted, copy until the end of the source array

    if (endIndex == -1)
    {
      endIndex = this.Count;
    }

    for (int index = startIndex; index < endIndex; index++)
    {
      dest.AddItem(this[index]);
    }
  }

  public virtual int AppendItem(TData item, bool allowUnsorted = true)
  {
    // Note: DiskSortedArray overrides this method and adds the item in sorted order
    EnsureLoaded();

    if (_arrayBlock.Count == _arrayBlock.MaxItems)
    {
      throw new Exception("DiskArray: Maximum array size exceeded");
    }

    DiskBlockManager.WriteDataBlock(
      Factory.DataBlockTypeIndex,
      _arrayBlock.DataAddress + _arrayBlock.DataSize * _arrayBlock.Count,
      ref item
    );

    _arrayBlock.Count++;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);
    DiskBlockManager.Flush();

    return _arrayBlock.Count - 1;
  }

  public void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out _arrayBlock);
      IsLoaded = true;
    }
  }

  public void GetAt(int index, out TData item)
  {
    EnsureLoaded();

    if (index < 0 || index > _arrayBlock.Count - 1)
    {
      throw new Exception($"DiskArray: Requested index is outside the bounds of the array, index = {index}");
    }

    DiskBlockManager.ReadDataBlock(
      Factory.DataBlockTypeIndex,
      _arrayBlock.DataAddress + _arrayBlock.DataSize * index,
      out item
    );
  }

  public void Grow(int growBy)
  {
    EnsureLoaded();

    if (_arrayBlock.Count + growBy > _arrayBlock.MaxItems)
    {
      throw new IndexOutOfRangeException(
        $"Growing the array would exceed the max items allowed. " +
          $"Count = {_arrayBlock.Count}, " +
          $"MaxItems = {_arrayBlock.MaxItems}"
      );
    }

    _arrayBlock.Count += growBy;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);
  }

  public void InsertAt(int index, TData item)
  {
    EnsureLoaded();
    Grow(1);
    ShiftRight(index, 1);
    this[index] = item;
  }

  public void SetAt(int index, TData item)
  {
    EnsureLoaded();

    // Note: We allow you to add new items to the end of the array so long as
    // the MaxItems property doesn't get exceeded.
    if (index < 0 || index > _arrayBlock.Count || index > _arrayBlock.MaxItems - 1)
    {
      throw new Exception("DiskArray: Requested index is outside the bounds of the array");
    }

    DiskBlockManager.WriteDataBlock(
      Factory.DataBlockTypeIndex,
      _arrayBlock.DataAddress + _arrayBlock.DataSize * index,
      ref item
    );

    if (index == _arrayBlock.Count)
    {
      // Client added a new item to end of list. Increment the Count;
      _arrayBlock.Count++;
      DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);
    }
  }

  public void ShiftRight(int startIndex, int spaces = 1)
  {
    EnsureLoaded();

    // Shift the elements to the right the specific number of spaces,
    // starting at index. The index is left unmodified.
    // Need a faster way to do this, but this should work for now
    for (int currentIndex = Count - 1; currentIndex >= startIndex + spaces; currentIndex--)
    {
      this[currentIndex] = this[currentIndex - spaces];
    }
  }

  public void Truncate(int count)
  {
    EnsureLoaded();
    if (count < _arrayBlock.Count)
    {
      _arrayBlock.Count = count;
      DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);
    }
  }
}
