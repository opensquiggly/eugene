namespace Eugene;

// File Format Structure
// ---------------------
// HBS   = Header Block Size - Controlled by us. SizeOf(Header)
// NBT   = Number of Requested Block Types - Specified by the consumer in the constructor.
// BTMDS = BlockTypeMetadataSize - Controlled by us. SizeOf(BlockTypeMetadata)
// BMDS  = BlockMetadataSize - Controlled by us. SizeOf(BlockMetadata).
//
//          Offset |----------------------------------------------------------------------------
//               0 |First comes the Header block which stores critical information about the
//                 |file, namely:
//                 |  * int HeaderBlockSize       - Size of header block
//                 |  * int ClientHeaderBlockSize - Size of client header block
//                 |  * int SchemaVersion         - Used in case we refactor anything
//                 |  * int BlockTypesCount       - Number of different block types tracked
//                 |  * int Data1                 - General purpose data storage for client
//                 |  * int Data2                 - General purpose data storage for client
//                 |  * int Data3                 - General purpose data storage for client
//                 |  * int Data4                 - General purpose data storage for client
//                 |  * long Address1             - General purpose address storage for client
//                 |  * long Address2             - General purpose address storage for client
//                 |  * long Address3             - General purpose address storage for client
//                 |  * long Address4             - General purpose address storage for client
//                 |  * long Address5             - General purpose address storage for client
//                 |  * long Address6             - General purpose address storage for client
//                 |  * long Address7             - General purpose address storage for client
//                 |  * long Address8             - General purpose address storage for client
//                 |----------------------------------------------------------------------------
//         HBS - 1 |Next is an array of structs of BlockTypeMetadataBlock that is used to store
//                 |information about each type of block stored in the file. The consumer
//                 |specifies how many different block types they need, and the size of each
//                 |block type that they want to store by passing parameters into the constructor.
//                 |This section occupies NBT * BTMDS bytes.
//                 |----------------------------------------------------------------------------
//           HBS + | . Data Blocks Follow From Here
// NBT * BTMDS - 1 | .
//                 | .
//                 |    -----------------------------------------------------------------------
//                 |    Each data block is prefixed by a BlockMetadata structure which stores
//                 |    the type of the block, and a next pointer which is only used when the
//                 |    block is freed and placed on the free list for that block type.
//                 |    -----------------------------------------------------------------------
//                 |    Data occupying the number of bytes corresponding to its block type
//                 |    -----------------------------------------------------------------------
//                 | .
//                 | .
//                 | . End of File
//                 |----------------------------------------------------------------------------

// Fix
public class DiskBlockManager : IDiskBlockManager, IDisposable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBlockManager()
  {
    RegisteredBlockTypes = new List<int>();

    CharBlockType = RegisterBlockType<char>();
    ShortBlockType = RegisterBlockType<short>();
    IntBlockType = RegisterBlockType<int>();
    LongBlockType = RegisterBlockType<long>();

    ArrayBlockType = RegisterBlockType<ArrayBlock>();
    BTreeBlockType = RegisterBlockType<BTreeBlock>();
    BTreeNodeBlockType = RegisterBlockType<BTreeNodeBlock>();
    LinkedListBlockType = RegisterBlockType<LinkedListBlock>();
    LinkedListNodeBlockType = RegisterBlockType<LinkedListNodeBlock>();

    ArrayManager = new DiskArrayManager(this, ArrayBlockType);
    SortedArrayManager = new DiskSortedArrayManager(this, ArrayBlockType);
    BTreeManager = new DiskBTreeManager(this, BTreeBlockType, BTreeNodeBlockType);
    FixedStringManager = new DiskFixedStringManager(this, ArrayBlockType);
    ImmutableStringManager = new DiskImmutableStringManager(this, ArrayBlockType);
    LinkedListManager = new DiskLinkedListManager(this, LinkedListBlockType, LinkedListNodeBlockType);

    ArrayOfShortFactory = ArrayManager.CreateFactory<short>(ShortBlockType);
    ArrayOfIntFactory = ArrayManager.CreateFactory<int>(IntBlockType);
    ArrayOfLongFactory = ArrayManager.CreateFactory<long>(LongBlockType);
    SortedArrayOfShortFactory = SortedArrayManager.CreateFactory<short>(ShortBlockType);
    SortedArrayOfIntFactory = SortedArrayManager.CreateFactory<int>(IntBlockType);
    SortedArrayOfLongFactory = SortedArrayManager.CreateFactory<long>(LongBlockType);    
    FixedStringFactory = FixedStringManager.CreateFactory(CharBlockType);
    ImmutableStringFactory = ImmutableStringManager.CreateFactory(CharBlockType);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private HeaderBlock _headerBlock;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static int BlockMetadataBlockSize => Marshal.SizeOf<BlockMetadataBlock>();

  private short ArrayBlockType { get; set; }

  public short CharBlockType { get; set; }

  public short ShortBlockType { get; set; }

  public short IntBlockType { get; set; }

  public short LongBlockType { get; set; }

  private short BTreeBlockType { get; set; }

  private short BTreeNodeBlockType { get; set; }

  private short LinkedListBlockType { get; set; }

  private short LinkedListNodeBlockType { get; set; }

  private short Fixed2KBlockType { get; set; }

  private short Fixed4KBlockType { get; set; }

  private DiskFixedStringManager FixedStringManager { get; set; }

  private DiskImmutableStringManager ImmutableStringManager { get; set; }

  public DiskArrayFactory<short> ArrayOfShortFactory { get; set; }

  public DiskArrayFactory<int> ArrayOfIntFactory { get; set; }

  public DiskArrayFactory<long> ArrayOfLongFactory { get; set; }
  
  public DiskSortedArrayFactory<short> SortedArrayOfShortFactory { get; set; }

  public DiskSortedArrayFactory<int> SortedArrayOfIntFactory { get; set; }

  public DiskSortedArrayFactory<long> SortedArrayOfLongFactory { get; set; }  

  public DiskLinkedListManager LinkedListManager { get; set; }

  public DiskArrayManager ArrayManager { get; set; }
  
  public DiskSortedArrayManager SortedArrayManager { get; set; }

  public DiskBTreeManager BTreeManager { get; set; }

  public DiskFixedStringFactory FixedStringFactory { get; }

  public DiskImmutableStringFactory ImmutableStringFactory { get; }

  public IList<BlockTypeMetadataBlock> BlockTypeMetadataBlocksList { get; set; }

  public IList<int> RegisteredBlockTypes { get; set; }

  private int HeaderBlockSize => Marshal.SizeOf<HeaderBlock>();

  private int ClientHeaderBlockSize { get; set; }

  private int BlockTypeMetadataBlockSize => Marshal.SizeOf<BlockTypeMetadataBlock>();

  private int BlockTypesCount { get; set; }

  private int TotalPreambleSize =>
    HeaderBlockSize + ClientHeaderBlockSize + BlockTypeMetadataBlockSize * BlockTypesCount;

  private string Path { get; set; }

  private FileStream FileStream { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void ReadRawBlock(long address, Span<byte> output)
  {
    FileStream.Seek(address, SeekOrigin.Begin);
    FileStream.Read(output);
  }

  private void ReadBlock<TStruct>(long address, out TStruct output) where TStruct : struct
  {
    int blockSize = Marshal.SizeOf<TStruct>();
    Span<byte> buffer = stackalloc byte[blockSize];
    ReadRawBlock(address, buffer);

    IntPtr ptr = Marshal.AllocHGlobal(blockSize);
    try
    {
      Marshal.Copy(buffer.ToArray(), 0, ptr, blockSize);
      output = Marshal.PtrToStructure<TStruct>(ptr);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
  }

  private void ReadHeaderBlock(out HeaderBlock output, bool replaceCachedCopy = true)
  {
    ReadBlock<HeaderBlock>(0, out output);
    if (replaceCachedCopy)
    {
      _headerBlock = output;
    }
  }

  private void ReadBlockTypeMetadataBlock(int blockTypeIndex, out BlockTypeMetadataBlock output,
    bool replaceCachedCopy = true)
  {
    ReadBlock<BlockTypeMetadataBlock>(
      HeaderBlockSize + ClientHeaderBlockSize + blockTypeIndex * BlockTypeMetadataBlockSize,
      out output
    );
    if (replaceCachedCopy)
    {
      BlockTypeMetadataBlocksList[blockTypeIndex] = output;
    }
  }

  private void ReadAllBlockTypeMetadataBlocks(ICollection<BlockTypeMetadataBlock> blockTypeMetadataBlocksList)
  {
    blockTypeMetadataBlocksList.Clear();

    for (int x = 0; x < BlockTypesCount; x++)
    {
      var blockTypeMetadataBlock = new BlockTypeMetadataBlock();
      ReadBlockTypeMetadataBlock(x, out blockTypeMetadataBlock, false);
      blockTypeMetadataBlocksList.Add(blockTypeMetadataBlock);
    }
  }

  private void ReadBlockMetadataBlock(long address, out BlockMetadataBlock input)
  {
    ReadBlock<BlockMetadataBlock>(address, out input);
  }

  private void WriteRawBlock(long address, Span<byte> input)
  {
    FileStream.Seek(address, SeekOrigin.Begin);
    FileStream.Write(input);
  }

  private void WriteBlock<TStruct>(long address, ref TStruct input) where TStruct : struct
  {
    int blockSize = Marshal.SizeOf<TStruct>();
    byte[] buffer = new byte[blockSize];

    IntPtr ptr = Marshal.AllocHGlobal(blockSize);
    try
    {
      Marshal.StructureToPtr(input, ptr, false);
      Marshal.Copy(ptr, buffer, 0, blockSize);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }

    WriteRawBlock(address, buffer);
  }

  public void WriteHeaderBlock(ref HeaderBlock input, bool replaceCachedCopy = true)
  {
    WriteBlock<HeaderBlock>(0, ref input);
    if (replaceCachedCopy)
    {
      _headerBlock = input;
    }
  }

  private void WriteBlockTypeMetadataBlock(int blockTypeIndex, ref BlockTypeMetadataBlock input,
    bool replaceCachedCopy = true)
  {
    WriteBlock<BlockTypeMetadataBlock>(
      HeaderBlockSize + ClientHeaderBlockSize + blockTypeIndex * BlockTypeMetadataBlockSize,
      ref input
    );
    if (replaceCachedCopy)
    {
      BlockTypeMetadataBlocksList[blockTypeIndex] = input;
    }
  }

  public void WriteAllBlockTypeMetadataBlocks(IList<BlockTypeMetadataBlock> blockTypeMetadataBlocks)
  {
    for (int index = 0; index < blockTypeMetadataBlocks.Count; index++)
    {
      BlockTypeMetadataBlock blockTypeMetadataBlock = blockTypeMetadataBlocks[index];
      WriteBlockTypeMetadataBlock(index, ref blockTypeMetadataBlock, false);
    }
  }

  private void WriteBlockMetadataBlock(long address, ref BlockMetadataBlock input)
  {
    WriteBlock<BlockMetadataBlock>(address, ref input);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public short RegisterBlockType<TData>() where TData : struct
  {
    int size = Marshal.SizeOf<TData>();
    RegisteredBlockTypes.Add(size);
    return (short) (RegisteredBlockTypes.Count - 1);
  }

  public DiskArrayFactory<TData> CreateArrayFactory<TData>(short dataBlockTypeIndex) where TData : struct, IComparable
  {
    return ArrayManager.CreateFactory<TData>(dataBlockTypeIndex);
  }

  public void CreateOrOpen(string filePath)
  {
    if (File.Exists(filePath))
    {
      Path = filePath;
      Open();
      ReadHeaderBlock(out _headerBlock);
      BlockTypesCount = RegisteredBlockTypes.Count;
      BlockTypeMetadataBlocksList = new List<BlockTypeMetadataBlock>();
      ReadAllBlockTypeMetadataBlocks(BlockTypeMetadataBlocksList);
    }
    else
    {
      FileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite);
      Path = filePath;

      var headerBlock = new HeaderBlock
      {
        HeaderBlockSize = HeaderBlockSize,
        ClientHeaderBlockSize = this.ClientHeaderBlockSize,
        SchemaVersion = 1,
        BlockTypesCount = RegisteredBlockTypes.Count
      };

      WriteHeaderBlock(ref headerBlock, true);
      BlockTypesCount = RegisteredBlockTypes.Count;
      BlockTypeMetadataBlocksList = new List<BlockTypeMetadataBlock>();

      foreach (int size in RegisteredBlockTypes)
      {
        var blockTypeMetadataBlock = new BlockTypeMetadataBlock() { ItemSize = size, FreeListHeadNode = 0 };
        BlockTypeMetadataBlocksList.Add(blockTypeMetadataBlock);
      }

      WriteAllBlockTypeMetadataBlocks(BlockTypeMetadataBlocksList);
    }
  }

  public void Open()
  {
    FileStream = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
  }

  public void Close()
  {
    if (FileStream != null)
    {
      Flush();
      FileStream.DisposeAsync();
      FileStream = null;
    }
  }

  public void Flush()
  {
    FileStream.Flush();
  }

  public void Dispose()
  {
    Flush();
    Close();
  }

  public HeaderBlock GetHeaderBlock() => this._headerBlock;

  public void ReadDataBlock<TStruct>(int blockTypeIndex, long address, out TStruct input) where TStruct : struct
  {
    if (BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize != Marshal.SizeOf<TStruct>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    ReadBlock<TStruct>(address + BlockMetadataBlockSize, out input);
  }

  public void ReadDataBlockArrayEntry<TStruct>(int blockTypeIndex, long baseAddress, int index, out TStruct input)
    where TStruct : struct
  {
    if (BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize != Marshal.SizeOf<TStruct>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    ReadBlock<TStruct>(
      baseAddress + BlockMetadataBlockSize + index * BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize, out input);
  }

  public void WriteDataBlockArrayEntry<TStruct>(int blockTypeIndex, long baseAddress, int index, ref TStruct input)
    where TStruct : struct
  {
    if (BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize != Marshal.SizeOf<TStruct>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    WriteBlock<TStruct>(
      baseAddress + BlockMetadataBlockSize + index * BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize, ref input);
  }

  public void WriteDataBlock<TStruct>(int blockTypeIndex, long address, ref TStruct input) where TStruct : struct
  {
    // Note that in the semantics used here, "address" is intended to represent the
    // logical address of where the block is stored on disk, which includes its
    // metadata. All data blocks are immediately preceded on the disk by a corresponding
    // BlockMetadataBlock to store information about the block, and to place it on a
    // free list of nodes in the event it is deleted.
    //
    // Therefore, the physical address of the data block is at address + BlockMetadataBlockSize.

    if (BlockTypeMetadataBlocksList[blockTypeIndex].ItemSize != Marshal.SizeOf<TStruct>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    WriteBlock<TStruct>(address + BlockMetadataBlockSize, ref input);
  }

  public long AppendDataBlock<TStruct>(int blockTypeIndex, ref TStruct input) where TStruct : struct
  {
    long address;
    BlockTypeMetadataBlock btmb = BlockTypeMetadataBlocksList[blockTypeIndex];

    if (btmb.ItemSize != Marshal.SizeOf<TStruct>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    if (btmb.FreeListHeadNode != 0)
    {
      // Take item off the free list and reuse it
      address = btmb.FreeListHeadNode;
      ReadBlockMetadataBlock(address, out BlockMetadataBlock freeBmb);
      btmb.FreeListHeadNode = freeBmb.NextBlock;
      WriteBlockTypeMetadataBlock(blockTypeIndex, ref btmb, true);
      freeBmb.Free = 0;
      freeBmb.NextBlock = 0;
      WriteBlockMetadataBlock(address, ref freeBmb);
      WriteDataBlock(blockTypeIndex, address, ref input);
    }
    else
    {
      // If there are no free blocks, add a new one at the end of the file
      address = FileStream.Length;
      BlockMetadataBlock newBmb = default;
      newBmb.Free = 0;
      newBmb.NextBlock = 0;
      WriteBlockMetadataBlock(address, ref newBmb);
      WriteDataBlock(blockTypeIndex, address, ref input);
    }

    Flush();

    return address;
  }

  public long AppendDataBlockArray<TData>(int blockTypeIndex, int count) where TData : struct
  {
    BlockTypeMetadataBlock btmb = BlockTypeMetadataBlocksList[blockTypeIndex];

    if (btmb.ItemSize != Marshal.SizeOf<TData>())
    {
      throw new Exception("The size of the provided structure does not match the size of the block type");
    }

    // if (btmb.FreeListHeadNode != 0)
    // {
    //   // Take item off the free list and reuse it
    //   address = btmb.FreeListHeadNode;
    //   ReadBlockMetadataBlock(address, out var freeBmb);
    //   btmb.FreeListHeadNode = freeBmb.NextBlock;
    //   WriteBlockTypeMetadataBlock(blockTypeIndex, ref btmb, true);
    //   freeBmb.Free = 0;
    //   freeBmb.NextBlock = 0;
    //   WriteBlockMetadataBlock(address, ref freeBmb);
    // }
    // else
    // {
    // If there are no free blocks, add a new one at the end of the file
    long address = FileStream.Length;
    BlockMetadataBlock newBmb = default;
    newBmb.Free = 0;
    newBmb.NextBlock = 0;
    WriteBlockMetadataBlock(address, ref newBmb);
    // }

    TData emptyData = default;
    for (int x = 0; x < count; x++)
    {
      // Write a series of empty data blocks <count> times.
      WriteDataBlock(blockTypeIndex, address + x * btmb.ItemSize, ref emptyData);
    }

    Flush();

    return address;
  }

  public void DeleteDataBlock(int blockTypeIndex, long address)
  {
    ReadBlockMetadataBlock(address, out BlockMetadataBlock bmb);
    bmb.NextBlock = BlockTypeMetadataBlocksList[blockTypeIndex].FreeListHeadNode;
    bmb.Free = 1;
    WriteBlockMetadataBlock(address, ref bmb);

    BlockTypeMetadataBlock btmb = BlockTypeMetadataBlocksList[blockTypeIndex];
    btmb.FreeListHeadNode = address;
    WriteBlockTypeMetadataBlock(blockTypeIndex, ref btmb, true);
  }
}
