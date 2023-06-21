namespace Eugene.Collections;

public class DiskArray<TData> where TData : struct, IComparable
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
  // Private Properties / Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private ArrayBlock _arrayBlock = default;

  private bool IsLoaded { get; set; } = false;

  private ArrayBlock ArrayBlock => _arrayBlock;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskArrayFactory<TData> Factory { get; }

  public int Count => GetCount();

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out _arrayBlock);
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

  public void AddItem(TData item)
  {
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

    _arrayBlock.Count++;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);

    DiskBlockManager.Flush();
  }

  public void GetAt(int index, out TData item)
  {
    EnsureLoaded();

    if (index < 0 || index > ArrayBlock.Count - 1)
    {
      throw new Exception("DiskArray: Requested index is outside the bounds of the array");
    }

    DiskBlockManager.ReadDataBlock(
      Factory.DataBlockTypeIndex,
      ArrayBlock.DataAddress + ArrayBlock.DataSize * index,
      out item
    );
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
      _arrayBlock.Count++;
      DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref _arrayBlock);
    }
  }

  public int GetCount()
  {
    EnsureLoaded();
    return _arrayBlock.Count;
  }
}
