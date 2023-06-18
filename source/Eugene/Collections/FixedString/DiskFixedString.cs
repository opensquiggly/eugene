using System.Text;
using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskFixedString
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskFixedString(DiskFixedStringFactory factory, long address)
  {
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public DiskFixedStringFactory Factory { get; }

  public int ArrayBlockTypeIndex => Factory.ArrayBlockTypeIndex;

  public int DataBlockTypeIndex => Factory.DataBlockTypeIndex;

  public long Address { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void SetValue(string val)
  {
    // Note: This does not handle Unicode characters larger than UTF-16
    // In other words, it doesn't handle multi-byte UTF-16. It assumes
    // each character consumes one 2-byte data block.
    //
    // Strings longer than the maximum declared length when the FixedString
    // was created originally are truncated, but no exception is thrown.

    DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out ArrayBlock block);

    int newLength = Math.Min(val.Length, block.MaxItems);
    for (int index = 0; index < newLength; index++)
    {
      char ch = val[index];
      DiskBlockManager.WriteDataBlockArrayEntry<char>(block.DataBlockTypeIndex, block.DataAddress, index, ref ch);
    }

    block.Count = newLength;
    DiskBlockManager.WriteDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, ref block);
  }

  public string GetValue()
  {
    var sb = new StringBuilder();

    DiskBlockManager.ReadDataBlock<ArrayBlock>(ArrayBlockTypeIndex, Address, out ArrayBlock block);

    for (int index = 0; index < block.Count; index++)
    {
      DiskBlockManager.ReadDataBlockArrayEntry<char>(DataBlockTypeIndex, block.DataAddress, index, out char ch);
      sb.Append(ch);
    }

    return sb.ToString();
  }
}
