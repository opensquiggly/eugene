namespace Eugene.Collections;

public class DiskSortedArrayManager : DiskArrayManager
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedArrayManager(IDiskBlockManager diskBlockManager, int arrayBlockTypeIndex) :
    base(diskBlockManager, arrayBlockTypeIndex)
  {
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public new DiskSortedArrayFactory<TData> CreateFactory<TData>(short dataBlockTypeIndex) where TData : struct, IComparable
  {
    return new DiskSortedArrayFactory<TData>(this, dataBlockTypeIndex);
  }
}