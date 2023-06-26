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
  // Protected Properties / Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  protected ArrayBlock ArrayBlock = default;

  protected bool IsLoaded { get; set; } = false;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskArrayFactory<TData> Factory { get; }

  public int Count => GetCount();

  public bool IsFull
  {
    get
    {
      EnsureLoaded();
      return this.Count >= ArrayBlock.MaxItems;
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Protected Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  protected void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out ArrayBlock);
      IsLoaded = true;
    }
  }

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

    if (ArrayBlock.Count == ArrayBlock.MaxItems)
    {
      throw new Exception("DiskArray: Maximum array size exceeded");
    }

    DiskBlockManager.WriteDataBlock(
      Factory.DataBlockTypeIndex,
      ArrayBlock.DataAddress + ArrayBlock.DataSize * ArrayBlock.Count,
      ref item
    );

    ArrayBlock.Count++;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref ArrayBlock);
    DiskBlockManager.Flush();

    return ArrayBlock.Count - 1;
  }

  public void GetAt(int index, out TData item)
  {
    EnsureLoaded();

    if (index < 0 || index > ArrayBlock.Count - 1)
    {
      throw new Exception($"DiskArray: Requested index is outside the bounds of the array, index = {index}");
    }

    DiskBlockManager.ReadDataBlock(
      Factory.DataBlockTypeIndex,
      ArrayBlock.DataAddress + ArrayBlock.DataSize * index,
      out item
    );
  }

  public int GetCount()
  {
    EnsureLoaded();
    return ArrayBlock.Count;
  }

  public void Grow(int growBy)
  {
    EnsureLoaded();

    if (ArrayBlock.Count + growBy > ArrayBlock.MaxItems)
    {
      throw new IndexOutOfRangeException(
        $"Growing the array would exceed the max items allowed. " +
          $"Count = {ArrayBlock.Count}, " +
          $"MaxItems = {ArrayBlock.MaxItems}"
      );
    }

    ArrayBlock.Count += growBy;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref ArrayBlock);
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
    if (index < 0 || index > ArrayBlock.Count || index > ArrayBlock.MaxItems - 1)
    {
      throw new Exception("DiskArray: Requested index is outside the bounds of the array");
    }

    DiskBlockManager.WriteDataBlock(
      Factory.DataBlockTypeIndex,
      ArrayBlock.DataAddress + ArrayBlock.DataSize * index,
      ref item
    );

    if (index == ArrayBlock.Count)
    {
      // Client added a new item to end of list. Increment the Count;
      ArrayBlock.Count++;
      DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref ArrayBlock);
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
    if (count < ArrayBlock.Count)
    {
      ArrayBlock.Count = count;
      DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref ArrayBlock);
    }
  }
}
