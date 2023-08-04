namespace Eugene.Collections;

public class DiskCompactByteListFactory
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteListFactory(DiskCompactByteListManager manager)
  {
    Manager = manager;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public DiskCompactByteListManager Manager { get; }

  public short CompactByteListBlockTypeIndex => Manager.CompactByteListBlockTypeIndex;
  
  public short Fixed16ByteBlockTypeIndex => Manager.Fixed16ByteBlockTypeIndex;

  public short Fixed32ByteBlockTypeIndex => Manager.Fixed32ByteBlockTypeIndex;

  public short Fixed64ByteBlockTypeIndex => Manager.Fixed64ByteBlockTypeIndex;

  public short Fixed128ByteBlockTypeIndex => Manager.Fixed128ByteBlockTypeIndex;

  public short Fixed256ByteBlockTypeIndex => Manager.Fixed256ByteBlockTypeIndex;

  public short Fixed512ByteBlockTypeIndex => Manager.Fixed512ByteBlockTypeIndex;

  public short Fixed1KBlockTypeIndex => Manager.Fixed1KBlockTypeIndex;

  public short Fixed2KBlockTypeIndex => Manager.Fixed2KBlockTypeIndex;

  public short Fixed4KBlockTypeIndex => Manager.Fixed4KBlockTypeIndex;

  public short Fixed8KBlockTypeIndex => Manager.Fixed8KBlockTypeIndex;

  public short Fixed16KBlockTypeIndex => Manager.Fixed16KBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteList AppendNew(byte[] data)
  {
    CompactByteListBlock block = default;
    block.HeadAddress = 0;
    block.TailAddress = 0;
    block.Count = 0;
    long address = DiskBlockManager.AppendDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, ref block);

    return new DiskCompactByteList(this, address);
  }

  public void Delete()
  {
    throw new NotImplementedException();
  }

  public DiskCompactByteList LoadExisting(long address)
  {
    throw new NotImplementedException();
  }  
}
