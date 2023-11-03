namespace Eugene.Collections;

public class DiskCompactByteListManager
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteListManager(
    IDiskBlockManager diskBlockManager,
    FixedByteBlockManager fixedByteBlockManager,
    short compactByteListBlockTypeIndex,
    short fixed16ByteBlockTypeIndex,
    short fixed32ByteBlockTypeIndex,
    short fixed64ByteBlockTypeIndex,
    short fixed128ByteBlockTypeIndex,
    short fixed256ByteBlockTypeIndex,
    short fixed512ByteBlockTypeIndex,
    short fixed1KByteBlockTypeIndex,
    short fixed2KBlockTypeIndex,
    short fixed4KBlockTypeIndex,
    short fixed8KBlockTypeIndex,
    short fixed16KBlockTypeIndex
  )
  {
    DiskBlockManager = diskBlockManager;
    FixedByteBlockManager = fixedByteBlockManager;
    CompactByteListBlockTypeIndex = compactByteListBlockTypeIndex;
    Fixed16ByteBlockTypeIndex = fixed16ByteBlockTypeIndex;
    Fixed32ByteBlockTypeIndex = fixed32ByteBlockTypeIndex;
    Fixed64ByteBlockTypeIndex = fixed64ByteBlockTypeIndex;
    Fixed128ByteBlockTypeIndex = fixed128ByteBlockTypeIndex;
    Fixed256ByteBlockTypeIndex = fixed256ByteBlockTypeIndex;
    Fixed512ByteBlockTypeIndex = fixed512ByteBlockTypeIndex;
    Fixed1KBlockTypeIndex = fixed1KByteBlockTypeIndex;
    Fixed2KBlockTypeIndex = fixed2KBlockTypeIndex;
    Fixed4KBlockTypeIndex = fixed4KBlockTypeIndex;
    Fixed8KBlockTypeIndex = fixed8KBlockTypeIndex;
    Fixed16KBlockTypeIndex = fixed16KBlockTypeIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager { get; }

  public FixedByteBlockManager FixedByteBlockManager { get; }

  public short CompactByteListBlockTypeIndex { get; }

  public short Fixed16ByteBlockTypeIndex { get; }

  public short Fixed32ByteBlockTypeIndex { get; }

  public short Fixed64ByteBlockTypeIndex { get; }

  public short Fixed128ByteBlockTypeIndex { get; }

  public short Fixed256ByteBlockTypeIndex { get; }

  public short Fixed512ByteBlockTypeIndex { get; }

  public short Fixed1KBlockTypeIndex { get; }

  public short Fixed2KBlockTypeIndex { get; }

  public short Fixed4KBlockTypeIndex { get; }

  public short Fixed8KBlockTypeIndex { get; }

  public short Fixed16KBlockTypeIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteListFactory CreateFactory()
  {
    return new DiskCompactByteListFactory(FixedByteBlockManager, this);
  }
}
