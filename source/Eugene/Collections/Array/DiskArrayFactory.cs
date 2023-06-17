using System.Runtime.InteropServices;
using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskArrayFactory<TData> where TData : struct, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskArrayFactory(DiskArrayManager manager, short dataBlockTypeIndex)
  {
    Manager = manager;
    DataBlockTypeIndex = dataBlockTypeIndex;
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public int ArrayBlockTypeIndex => Manager.ArrayBlockTypeIndex;

  public short DataBlockTypeIndex { get; }
  
  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager; 

  public DiskArrayManager Manager { get; }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  public DiskArray<TData> AppendNew(int maxItems)
  {
    long dataAddress = DiskBlockManager.AppendDataBlockArray<TData>(DataBlockTypeIndex, maxItems);
    ArrayBlock block = default;
    block.DataBlockTypeIndex = this.DataBlockTypeIndex;
    block.DataSize = Marshal.SizeOf<TData>();
    block.MaxItems = maxItems;
    block.Count = 0;
    block.DataAddress = dataAddress;
    
    long address = DiskBlockManager.AppendDataBlock<ArrayBlock>(ArrayBlockTypeIndex, ref block);
    return new DiskArray<TData>(this, address);
  }

  public void Delete()
  {
    throw new NotImplementedException();
  }
  
  public DiskArray<TData> LoadExisting(long address)
  {
    return new DiskArray<TData>(this, address);
  }
}
