namespace Eugene.Collections;

public class DiskCompactByteList
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteList(DiskCompactByteListFactory factory, long address)
  {
    Factory = factory;
    Address = address;
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; private set; }

  public DiskCompactByteListFactory Factory { get; }

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;
  
  public short CompactByteListBlockTypeIndex => Factory.CompactByteListBlockTypeIndex;
  
  public short Fixed16ByteBlockTypeIndex => Factory.Fixed16ByteBlockTypeIndex;

  public short Fixed32ByteBlockTypeIndex => Factory.Fixed32ByteBlockTypeIndex;

  public short Fixed64ByteBlockTypeIndex => Factory.Fixed64ByteBlockTypeIndex;

  public short Fixed128ByteBlockTypeIndex => Factory.Fixed128ByteBlockTypeIndex;

  public short Fixed256ByteBlockTypeIndex => Factory.Fixed256ByteBlockTypeIndex;

  public short Fixed512ByteBlockTypeIndex => Factory.Fixed512ByteBlockTypeIndex;

  public short Fixed1KByteBlockTypeIndex => Factory.Fixed1KBlockTypeIndex;

  public short Fixed2KByteBlockTypeIndex => Factory.Fixed2KBlockTypeIndex;

  public short Fixed4KByteBlockTypeIndex => Factory.Fixed4KBlockTypeIndex;

  public short Fixed8KByteBlockTypeIndex => Factory.Fixed8KBlockTypeIndex;

  public short Fixed16KByteBlockTypeIndex => Factory.Fixed16KBlockTypeIndex;
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static List<FixedSizeBlockInfo> CompressBlocks(List<FixedSizeBlockInfo> blocks)
  {
    List<FixedSizeBlockInfo> tempBlocks = blocks.Select(block => new FixedSizeBlockInfo(block.BlockSize, block.BytesStored)).ToList();
    List<int> bytesToTransfer = new List<int>();

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

  private static IFixedByteBlock CreateFixedByteBlock(int size)
  {
    switch (size)
    {
      case 16:    return new Fixed16ByteBlock();
      case 32:    return new Fixed32ByteBlock();
      case 64:    return new Fixed64ByteBlock();
      case 128:   return new Fixed128ByteBlock();
      case 256:   return new Fixed256ByteBlock();
      case 512:   return new Fixed512ByteBlock();
      case 1024:  return new Fixed1KByteBlock();
      case 2048:  return new Fixed2KByteBlock();
      case 4096:  return new Fixed4KByteBlock();
      case 8192:  return new Fixed8KByteBlock();
      case 16384: return new Fixed16KByteBlock();
      default: throw new ArgumentOutOfRangeException("Expected a power of 2 from 16 to 16384");
    }
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private long AppendFixedSizeBlock(IFixedByteBlock fixedByteBlock)
  {
    switch (fixedByteBlock.Size)
    {
      case 16:
        {
          Fixed16ByteBlock typedBlock = (Fixed16ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed16ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 32:
        {
          Fixed32ByteBlock typedBlock = (Fixed32ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed32ByteBlockTypeIndex, ref typedBlock);
        }  
      
      case 64:
        {
          Fixed64ByteBlock typedBlock = (Fixed64ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed64ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 128:
        {
          Fixed128ByteBlock typedBlock = (Fixed128ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed128ByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 256:
        {
          Fixed256ByteBlock typedBlock = (Fixed256ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed256ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 512:
        {
          Fixed512ByteBlock typedBlock = (Fixed512ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed512ByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 1024:
        {
          Fixed1KByteBlock typedBlock = (Fixed1KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed1KByteBlockTypeIndex, ref typedBlock);
        }  
      
      case 2048:
        {
          Fixed2KByteBlock typedBlock = (Fixed2KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed2KByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 4096:
        {
          Fixed4KByteBlock typedBlock = (Fixed4KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed4KByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 8192:
        {
          Fixed8KByteBlock typedBlock = (Fixed8KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed8KByteBlockTypeIndex, ref typedBlock);
        }
      
      case 16384:
        {
          Fixed16KByteBlock typedBlock = (Fixed16KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed16KByteBlockTypeIndex, ref typedBlock);
        }          
      
      default: throw new ArgumentOutOfRangeException("Expected fixedByteBlock.Size to be a power of 2 from 16 to 16384");
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
    List<FixedSizeBlockInfo> blocksUsed = new List<FixedSizeBlockInfo>();

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

  public unsafe void AppendData(byte[] data)
  {
    DiskBlockManager.ReadDataBlock<CompactByteListBlock>(CompactByteListBlockTypeIndex, Address, out CompactByteListBlock block);
    
    // If there is a tail block, and the data can fit in it, then append the data to the end of the tail block
    // If there is a tail block and the data can't fit into it, append a series of blocks to the end
    // If there isn't a tail block, then append a series of blocks

    if (block.TailAddress == 0)
    {
      List<FixedSizeBlockInfo> blockInfoList = AllocateBlocks(data.Length);
      int offset = 0;

      foreach (FixedSizeBlockInfo blockInfo in blockInfoList)
      {
        IFixedByteBlock fixedByteBlock = CreateFixedByteBlock(blockInfo.BlockSize);
        Marshal.Copy(data, offset, (IntPtr) fixedByteBlock.DataPointer, blockInfo.BytesStored);
        // Cases to handle
        // 1) First block ever inserted into list
        //    a) Set CompactByteListBlock's HeadAddress and TailAddress to the new block
        //    b) Set the fixedByteBlock's PreviousAddress and Next Address to zero
        //    c) Set the CompactByteListBlock's Count to the fixedByteBlock's BytesStored property
        // 2) First block of this AppendData operation, but not the first block in the list (TailAddress != 0)
        //    a) Read the fixedByteBlock referenced by CaompactByteListBlock's current TailAddress
        //    b) Set CompactByteListBlock's TailAddress to the new block
        //    c) Set previous blocks NextAddress to current block
        //    d) Set current block's PreviousAddress to last block
        AppendFixedSizeBlock(fixedByteBlock);
        offset += blockInfo.BytesStored;
      }
    }
    else
    {
      throw new NotImplementedException();
    }
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Inner Classes
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public class FixedSizeBlockInfo {
    public int BlockSize { get; set; }
    public int BytesStored { get; set; }

    public FixedSizeBlockInfo(int blockSize, int bytesStored) {
      BlockSize = blockSize;
      BytesStored = bytesStored;
    }
  }
}
