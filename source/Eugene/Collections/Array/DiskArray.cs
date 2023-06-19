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
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskArrayFactory<TData> Factory { get; }

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
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void AddItem(TData item)
  {
    DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out ArrayBlock block);
    if (block.Count == block.MaxItems)
    {
      throw new Exception("DiskArray: Maximum array size exceeded");
    }

    DiskBlockManager.WriteDataBlock(
      Factory.DataBlockTypeIndex,
      block.DataAddress + block.DataSize * block.Count,
      ref item
    );

    block.Count++;
    DiskBlockManager.WriteDataBlock(ArrayBlockTypeIndex, Address, ref block);

    DiskBlockManager.Flush();
  }

  public void GetAt(int index, out TData item)
  {
    DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out ArrayBlock block);
    if (index < 0 || index > block.Count - 1)
    {
      throw new Exception("DiskArray: Requested index is outside the bounds of the array");
    }

    DiskBlockManager.ReadDataBlock(
      Factory.DataBlockTypeIndex,
      block.DataAddress + block.DataSize * index,
      out item
    );
  }
}
