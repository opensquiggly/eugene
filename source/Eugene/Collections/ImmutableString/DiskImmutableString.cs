using System.Text;
using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskImmutableString
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableString(DiskImmutableStringFactory factory, long address)
  {
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskImmutableStringFactory Factory { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public int DataBlockTypeIndex => Factory.DataBlockTypeIndex;

  public long Address { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public string GetValue()
  {
    StringBuilder sb = new StringBuilder();

    DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out var block);

    for (int index = 0; index < block.Count; index++)
    {
      DiskBlockManager.ReadDataBlockArrayEntry<char>(DataBlockTypeIndex, block.DataAddress, index, out char ch);
      sb.Append(ch);
    }

    return sb.ToString();
  }
}
