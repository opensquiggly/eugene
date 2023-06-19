namespace Eugene.Collections;

public class DiskFixedStringFactory
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskFixedStringFactory(DiskFixedStringManager manager, short dataBlockTypeIndex)
  {
    Manager = manager;
    DataBlockTypeIndex = dataBlockTypeIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskFixedStringManager Manager { get; }

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public short DataBlockTypeIndex { get; }

  public int ArrayBlockTypeIndex => Manager.ArrayBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskFixedString AppendEmpty(int maxLength)
  {
    long dataAddress = DiskBlockManager.AppendDataBlockArray<char>(DataBlockTypeIndex, maxLength);
    ArrayBlock block = default;
    block.DataBlockTypeIndex = this.DataBlockTypeIndex;
    block.DataSize = Marshal.SizeOf<char>();
    block.MaxItems = maxLength;
    block.Count = 0;
    block.DataAddress = dataAddress;

    long address = DiskBlockManager.AppendDataBlock<ArrayBlock>(ArrayBlockTypeIndex, ref block);

    return new DiskFixedString(this, address);
  }

  public DiskFixedString LoadExisting(long address)
  {
    return new DiskFixedString(this, address);
  }
}
