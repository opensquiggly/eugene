using System.Runtime.InteropServices;
using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskImmutableStringFactory
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableStringFactory(DiskImmutableStringManager manager, short dataBlockTypeIndex)
  {
    Manager = manager;
    DataBlockTypeIndex = dataBlockTypeIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableStringManager Manager { get; }

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public short DataBlockTypeIndex { get; }

  public int ArrayBlockTypeIndex => Manager.ArrayBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableString Append(string val)
  {
    long dataAddress = DiskBlockManager.AppendDataBlockArray<char>(DataBlockTypeIndex, val.Length);
    for (int index = 0; index < val.Length; index++)
    {
      char ch = val[index];
      DiskBlockManager.WriteDataBlockArrayEntry<char>(DataBlockTypeIndex, dataAddress, index, ref ch);
    }

    ArrayBlock block = default;
    block.DataBlockTypeIndex = DataBlockTypeIndex;
    block.DataSize = Marshal.SizeOf<char>();
    block.MaxItems = val.Length;
    block.Count = val.Length;
    block.DataAddress = dataAddress;

    long address = DiskBlockManager.AppendDataBlock<ArrayBlock>(ArrayBlockTypeIndex, ref block);
    return new DiskImmutableString(this, address);
  }

  public DiskImmutableString LoadExisting(long address)
  {
    return new DiskImmutableString(this, address);
  }
}
