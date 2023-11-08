namespace Eugene.Collections;

public class DiskCompactByteListFactory
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteListFactory(FixedByteBlockManager fixedByteBlockManager, DiskCompactByteListManager manager)
  {
    FixedByteBlockManager = fixedByteBlockManager;
    Manager = manager;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public FixedByteBlockManager FixedByteBlockManager { get; }

  public DiskCompactByteListManager Manager { get; }

  public short CompactByteListBlockTypeIndex => Manager.CompactByteListBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteList AppendNew(byte[] data = null)
  {
    CompactByteListBlock block = default;
    block.HeadAddress = 0;
    block.TailAddress = 0;
    block.Count = 0;
    long address = DiskBlockManager.AppendDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, ref block);

    var result = new DiskCompactByteList(FixedByteBlockManager, this, address);
    if (data != null)
    {
      // TODO: Figure out why this doesn't work
      // result.AppendData(data);
    }

    return result;
  }

  public void Delete()
  {
    throw new NotImplementedException();
  }

  public DiskCompactByteList LoadExisting(long address)
  {
    return new DiskCompactByteList(FixedByteBlockManager, this, address);
  }
}
