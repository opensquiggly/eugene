namespace Eugene.Collections;

public class DiskSortedVarIntList : IFastEnumerable<IFastEnumerator<ulong, long>, ulong, long>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntList(DiskCompactByteList baseList, FixedByteBlockManager fixedByteBlockManager, DiskSortedVarIntListFactory factory)
  {
    BaseList = baseList;
    FixedByteBlockManager = fixedByteBlockManager;
    Factory = factory;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool IsLoaded { get; set; } = false;

  private bool IsInitializing { get; set; } = false;

  private ulong LastValue { get; set; } = 0;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address => BaseList.Address;

  public FixedByteBlockManager FixedByteBlockManager { get; }

  public DiskCompactByteList BaseList { get; }

  public DiskSortedVarIntListFactory Factory { get; }

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public short CompactByteListBlockTypeIndex => Factory.CompactByteListManager.CompactByteListBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private int CalculateConvertedBytesSize(ulong value)
  {
    int size = 0;
    do
    {
      size++;
      value >>= 7;
    } while (value != 0);
    return size;
  }

  private int ConvertValueToBytes(ulong value, byte[] output1, byte[] output2, int startingOffset, out bool didOverflow)
  {
    // Note: Uses ULEB128 encoding to store an arbitrarily sized unsigned long
    // See https://en.wikipedia.org/wiki/LEB128

    didOverflow = false;
    int currentOffset = startingOffset;
    byte[] output = output1;

    do
    {
      if (currentOffset >= output.Length)
      {
        if (!didOverflow)
        {
          output = output2;
          didOverflow = true;
          currentOffset = 0;
        }
        else
        {
          throw new ArgumentException("Overflow array is not large enough to hold the encoded value.");
        }
      }

      byte b = (byte) (value & 0x7F); // Take the 7 least significant bits
      value >>= 7; // Shift right by 7 bits
      if (value != 0) // If there are more bits to encode, set the continuation bit
      {
        b |= 0x80;
      }
      output[currentOffset] = b;
      currentOffset++;
    } while (value != 0);

    return currentOffset;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void EnsureLoaded()
  {
    if (!IsLoaded && !IsInitializing)
    {
      BaseList.EnsureLoaded();

      if (!IsInitializing)
      {
        IsInitializing = true;
        var cursor = new DiskSortedVarIntListCursor(this);
        cursor.Reset();
        LastValue = 0;
        while (cursor.MoveNext())
        {
          LastValue = (cursor.Current == null) ? 0L : (ulong) cursor.Current;
        }
      }

      IsInitializing = false;
      IsLoaded = true;
    }
  }

  public void AppendData(IEnumerable<ulong> data)
  {
    EnsureLoaded();

    byte[] buffer1 = new byte[15360]; // 15/16ths of 16384
    byte[] buffer2 = new byte[15360];
    byte[] mainBuffer = buffer1;
    byte[] overflowBuffer = buffer2;
    int currentIndex = 0;

    foreach (ulong val in data)
    {
      if (val < LastValue)
      {
        throw new ArgumentOutOfRangeException("Expect values to be in sorted order");
      }

      ulong currentDiffValue = val - LastValue;
      currentIndex = ConvertValueToBytes(currentDiffValue, mainBuffer, overflowBuffer, currentIndex, out bool didOverflow);
      if (didOverflow)
      {
        BaseList.AppendData(mainBuffer, mainBuffer.Length);
        (mainBuffer, overflowBuffer) = (overflowBuffer, mainBuffer);
      }
      LastValue = val;
    }

    if (currentIndex > 0)
    {
      BaseList.AppendData(mainBuffer, currentIndex);
    }
  }

  public IFixedByteBlock ReadBlock(long address)
  {
    return BaseList.ReadBlock(address);
  }

  public IFixedByteBlock ReadHeadBlock()
  {
    return BaseList.ReadHeadBlock();
  }

  public IFastEnumerator<ulong, long> GetFastEnumerator()
  {
    return new DiskSortedVarIntListCursor(this);
  }

  public IEnumerator<ulong> GetEnumerator()
  {
    return new DiskSortedVarIntListCursor(this);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }
}
