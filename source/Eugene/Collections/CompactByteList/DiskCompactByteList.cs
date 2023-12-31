namespace Eugene.Collections;

public class DiskCompactByteList
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteList(FixedByteBlockManager fixedByteBlockManager, DiskCompactByteListFactory factory, long address)
  {
    FixedByteBlockManager = fixedByteBlockManager;
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool IsLoaded { get; set; } = false;

  private CompactByteListBlock _listBlock = default;

  private IFixedByteBlock HeadBlock { get; set; } = null;

  private IFixedByteBlock TailBlock { get; set; } = null;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; private set; }

  public FixedByteBlockManager FixedByteBlockManager { get; }

  public DiskCompactByteListFactory Factory { get; }

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public short CompactByteListBlockTypeIndex => Factory.CompactByteListBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static List<FixedSizeBlockInfo> CompressBlocks(IEnumerable<FixedSizeBlockInfo> blocks)
  {
    var tempBlocks = blocks.Select(block => new FixedSizeBlockInfo(block.BlockSize, block.BytesStored)).ToList();
    var bytesToTransfer = new List<int>();

    for (int i = 0; i < tempBlocks.Count - 1; i++)
    {
      FixedSizeBlockInfo currentFixedSizeBlockInfo = tempBlocks[i];

      // First pass: check if it's possible to reallocate all bytes from the current block
      int remainingBytes = currentFixedSizeBlockInfo.BytesStored;
      bytesToTransfer.Clear();

      for (int j = i + 1; j < tempBlocks.Count && remainingBytes > 0; j++)
      {
        FixedSizeBlockInfo nextFixedSizeBlockInfo = tempBlocks[j];

        // Calculate the maximum bytes we can add to the next block
        int maxAddableBytes = Math.Max(0, (nextFixedSizeBlockInfo.BlockSize * 15 / 16) - nextFixedSizeBlockInfo.BytesStored);
        int transferBytes = Math.Min(maxAddableBytes, remainingBytes);
        bytesToTransfer.Add(transferBytes);
        remainingBytes -= transferBytes;
      }

      if (remainingBytes != 0)
      {
        // We cannot fully reallocate the block into a bigger block, so leave
        // the currrent block unmodified and quit. We're done compressing.
        break;
      }

      // Reallocate the current block into bigger blocks
      for (int j = 0; j < bytesToTransfer.Count; j++)
      {
        FixedSizeBlockInfo nextFixedSizeBlockInfo = tempBlocks[i + 1 + j];
        nextFixedSizeBlockInfo.BytesStored += bytesToTransfer[j];
      }

      currentFixedSizeBlockInfo.BytesStored = 0;
    }

    // Filter out any blocks that we emptied
    tempBlocks.RemoveAll(block => block.BytesStored == 0);

    return tempBlocks;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private unsafe void AppendDataAsSeriesOfBlocks(byte[] data, int length)
  {
    List<FixedSizeBlockInfo> blockInfoList = AllocateBlocks(length);
    int offset = 0;

    foreach (FixedSizeBlockInfo blockInfo in blockInfoList)
    {
      IFixedByteBlock fixedByteBlock = FixedByteBlock.CreateFixedByteBlock(blockInfo.BlockSize);
      Marshal.Copy(data, offset, (IntPtr) fixedByteBlock.DataPointer, blockInfo.BytesStored);
      fixedByteBlock.NextAddress = 0;
      fixedByteBlock.PreviousAddress = _listBlock.TailAddress;
      fixedByteBlock.BytesStored = (ushort) blockInfo.BytesStored;
      long newBlockAddress = FixedByteBlockManager.AppendFixedSizeBlock(fixedByteBlock);

      if (TailBlock == null)
      {
        _listBlock.HeadAddress = newBlockAddress;
        _listBlock.TailAddress = newBlockAddress;
      }
      else
      {
        TailBlock.NextAddress = newBlockAddress;
        FixedByteBlockManager.WriteFixedSizeBlock(_listBlock.TailAddress, TailBlock);
        _listBlock.TailAddress = newBlockAddress;
      }

      _listBlock.Count += blockInfo.BytesStored;
      DiskBlockManager.WriteDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, Address, ref _listBlock);
      TailBlock = fixedByteBlock;
      offset += blockInfo.BytesStored;
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Static Methods - Exposed for Unit Testing Only
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public static List<FixedSizeBlockInfo> AllocateBlocks(int numberOfBytes)
  {
    return CompressBlocks(AllocateRawBlocks(numberOfBytes));
  }

  public static List<FixedSizeBlockInfo> AllocateRawBlocks(int numberOfBytes)
  {
    var blocksUsed = new List<FixedSizeBlockInfo>();

    while (numberOfBytes > 0)
    {
      int blockSize;
      int bytesToUse;

      switch (numberOfBytes)
      {
        case >= 14336:
          blockSize = 16384;
          bytesToUse = 14336;
          break;

        case >= 7168:
          blockSize = 8192;
          bytesToUse = 7168;
          break;

        case >= 3584:
          blockSize = 4096;
          bytesToUse = 3584;
          break;

        case >= 1792:
          blockSize = 2048;
          bytesToUse = 1792;
          break;

        case >= 896:
          blockSize = 1024;
          bytesToUse = 896;
          break;

        case >= 448:
          blockSize = 512;
          bytesToUse = 448;
          break;

        case >= 224:
          blockSize = 256;
          bytesToUse = 224;
          break;

        case >= 112:
          blockSize = 128;
          bytesToUse = 112;
          break;

        case >= 56:
          blockSize = 64;
          bytesToUse = 56;
          break;

        case >= 28:
          blockSize = 32;
          bytesToUse = 28;
          break;

        case >= 16:
          blockSize = 16;
          bytesToUse = 16;
          break;

        default:
          blockSize = 16;
          bytesToUse = numberOfBytes;
          break;
      }

      blocksUsed.Add(new FixedSizeBlockInfo(blockSize, bytesToUse));
      numberOfBytes -= bytesToUse;
    }

    blocksUsed.Reverse();
    return blocksUsed;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, Address, out _listBlock);

      if (_listBlock.HeadAddress != 0)
      {
        HeadBlock = ReadBlock(_listBlock.HeadAddress);
      }

      if (_listBlock.TailAddress != 0)
      {
        TailBlock = ReadBlock(_listBlock.TailAddress);
      }

      IsLoaded = true;
    }
  }

  public unsafe void AppendData(byte[] data, int length)
  {
    EnsureLoaded();

    if (TailBlock == null)
    {
      AppendDataAsSeriesOfBlocks(data, length);
    }
    else
    {
      if (TailBlock.Size - TailBlock.BytesStored < length)
      {
        // No room in the last block, so append new blocks as per usual
        AppendDataAsSeriesOfBlocks(data, length);
      }
      else
      {
        // New data fits in the last block, so just write it there
        Marshal.Copy(data, 0, (IntPtr) TailBlock.DataPointer + TailBlock.BytesStored, length);
        TailBlock.BytesStored += (ushort) length;
        FixedByteBlockManager.WriteFixedSizeBlock(_listBlock.TailAddress, TailBlock);
      }
    }
  }

  public IFixedByteBlock ReadBlock(long address)
  {
    DiskBlockManager.ReadBlockMetadataBlock(address, out BlockMetadataBlock blockMetadataBlock);
    int blockSize = FixedByteBlockManager.GetFixedByteBlockSize(blockMetadataBlock.BlockTypeId);
    IFixedByteBlock fixedByteBlock = FixedByteBlock.CreateFixedByteBlock(blockSize);
    FixedByteBlockManager.ReadFixedSizeBlock(address, fixedByteBlock);

    return fixedByteBlock;
  }

  public IFixedByteBlock ReadHeadBlock()
  {
    // TODO: Implement EnsureLoaded() and use that instead of loading the block
    DiskBlockManager.ReadDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, Address, out CompactByteListBlock block);

    if (block.HeadAddress == 0)
    {
      return null;
    }

    DiskBlockManager.ReadBlockMetadataBlock(block.HeadAddress, out BlockMetadataBlock blockMetadataBlock);
    int blockSize = FixedByteBlockManager.GetFixedByteBlockSize(blockMetadataBlock.BlockTypeId);
    IFixedByteBlock fixedByteBlock = FixedByteBlock.CreateFixedByteBlock(blockSize);
    FixedByteBlockManager.ReadFixedSizeBlock(block.HeadAddress, fixedByteBlock);

    return fixedByteBlock;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Inner Classes
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public class FixedSizeBlockInfo
  {
    public int BlockSize { get; set; }
    public int BytesStored { get; set; }

    public FixedSizeBlockInfo(int blockSize, int bytesStored)
    {
      BlockSize = blockSize;
      BytesStored = bytesStored;
    }
  }
}
