namespace Eugene.Collections;

public class DiskSortedArrayFactory<TData> : DiskArrayFactory<TData>
  where TData : struct, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedArrayFactory(DiskSortedArrayManager manager, short dataBlockTypeIndex) : base(manager, dataBlockTypeIndex)
  {
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public new DiskSortedArray<TData> AppendNew(int maxItems)
  {
    long dataAddress = DiskBlockManager.AppendDataBlockArray<TData>(DataBlockTypeIndex, maxItems);
    ArrayBlock block = default;
    block.DataBlockTypeIndex = this.DataBlockTypeIndex;
    block.DataSize = Marshal.SizeOf<TData>();
    block.MaxItems = maxItems;
    block.Count = 0;
    block.DataAddress = dataAddress;

    long address = DiskBlockManager.AppendDataBlock<ArrayBlock>(ArrayBlockTypeIndex, ref block);
    return new DiskSortedArray<TData>(this, address);
  }

  public new void Delete()
  {
    throw new NotImplementedException();
  }

  public new DiskSortedArray<TData> LoadExisting(long address)
  {
    return new DiskSortedArray<TData>(this, address);
  }
}
