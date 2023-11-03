namespace Eugene.Helpers;

public class FixedByteBlockManager
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FixedByteBlockManager(
    IDiskBlockManager diskBlockManager, 
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
    Fixed16ByteBlockTypeIndex = fixed16ByteBlockTypeIndex;
    Fixed32ByteBlockTypeIndex = fixed32ByteBlockTypeIndex;
    Fixed64ByteBlockTypeIndex = fixed64ByteBlockTypeIndex;
    Fixed128ByteBlockTypeIndex = fixed128ByteBlockTypeIndex;
    Fixed256ByteBlockTypeIndex = fixed256ByteBlockTypeIndex;
    Fixed512ByteBlockTypeIndex = fixed512ByteBlockTypeIndex;
    Fixed1KByteBlockTypeIndex = fixed1KByteBlockTypeIndex;
    Fixed2KByteBlockTypeIndex = fixed2KBlockTypeIndex;
    Fixed4KByteBlockTypeIndex = fixed4KBlockTypeIndex;
    Fixed8KByteBlockTypeIndex = fixed8KBlockTypeIndex;
    Fixed16KByteBlockTypeIndex = fixed16KBlockTypeIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager { get; }
  
  public short Fixed16ByteBlockTypeIndex { get; }
  
  public short Fixed32ByteBlockTypeIndex { get; }
  
  public short Fixed64ByteBlockTypeIndex { get; }
  
  public short Fixed128ByteBlockTypeIndex { get; }
  
  public short Fixed256ByteBlockTypeIndex { get; }
  
  public short Fixed512ByteBlockTypeIndex { get; }
  
  public short Fixed1KByteBlockTypeIndex { get; }
  
  public short Fixed2KByteBlockTypeIndex { get; }
  
  public short Fixed4KByteBlockTypeIndex { get; }
  
  public short Fixed8KByteBlockTypeIndex { get; }
  
  public short Fixed16KByteBlockTypeIndex { get; }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  public int GetFixedByteBlockSize(int blockTypeIndex)
  {
    switch (blockTypeIndex)
    {
      case var _ when blockTypeIndex == Fixed16ByteBlockTypeIndex:
        return 16;

      case var _ when blockTypeIndex == Fixed32ByteBlockTypeIndex:
        return 32;
      
      case var _ when blockTypeIndex == Fixed64ByteBlockTypeIndex:
        return 64;   
      
      case var _ when blockTypeIndex == Fixed128ByteBlockTypeIndex:
        return 128; 
      
      case var _ when blockTypeIndex == Fixed256ByteBlockTypeIndex:
        return 256;
      
      case var _ when blockTypeIndex == Fixed512ByteBlockTypeIndex:
        return 512; 
      
      case var _ when blockTypeIndex == Fixed1KByteBlockTypeIndex:
        return 1024; 
      
      case var _ when blockTypeIndex == Fixed2KByteBlockTypeIndex:
        return 2048;  
      
      case var _ when blockTypeIndex == Fixed4KByteBlockTypeIndex:
        return 4096; 
      
      case var _ when blockTypeIndex == Fixed8KByteBlockTypeIndex:
        return 8192;   
      
      case var _ when blockTypeIndex == Fixed16KByteBlockTypeIndex:
        return 16384;          

      default:
        return 0;
    }
  }  
  
  public long AppendFixedSizeBlock(IFixedByteBlock fixedByteBlock)
  {
    switch (fixedByteBlock.Size)
    {
      case 16:
        {
          var typedBlock = (Fixed16ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed16ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 32:
        {
          var typedBlock = (Fixed32ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed32ByteBlockTypeIndex, ref typedBlock);
        }  
      
      case 64:
        {
          var typedBlock = (Fixed64ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed64ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 128:
        {
          var typedBlock = (Fixed128ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed128ByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 256:
        {
          var typedBlock = (Fixed256ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed256ByteBlockTypeIndex, ref typedBlock);
        }
      
      case 512:
        {
          var typedBlock = (Fixed512ByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed512ByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 1024:
        {
          var typedBlock = (Fixed1KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed1KByteBlockTypeIndex, ref typedBlock);
        }  
      
      case 2048:
        {
          var typedBlock = (Fixed2KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed2KByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 4096:
        {
          var typedBlock = (Fixed4KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed4KByteBlockTypeIndex, ref typedBlock);
        } 
      
      case 8192:
        {
          var typedBlock = (Fixed8KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed8KByteBlockTypeIndex, ref typedBlock);
        }
      
      case 16384:
        {
          var typedBlock = (Fixed16KByteBlock) fixedByteBlock;
          return DiskBlockManager.AppendDataBlock(Fixed16KByteBlockTypeIndex, ref typedBlock);
        }          
      
      default: throw new ArgumentOutOfRangeException("Expected fixedByteBlock.Size to be a power of 2 from 16 to 16384");
    }
  }

  public unsafe void ReadFixedSizeBlock(long address, IFixedByteBlock fixedByteBlock)
  {
    IFixedByteBlock tempFixedByteBlock;
    
    switch (fixedByteBlock.Size)
    {
      case 16: 
        DiskBlockManager.ReadDataBlock(Fixed16ByteBlockTypeIndex, address, out Fixed16ByteBlock typed16Block);
        tempFixedByteBlock = typed16Block;
        break;
      
      case 32:
        DiskBlockManager.ReadDataBlock(Fixed32ByteBlockTypeIndex, address, out Fixed32ByteBlock typed32Block);
        tempFixedByteBlock = typed32Block;
        break;
      
      case 64:
        DiskBlockManager.ReadDataBlock(Fixed64ByteBlockTypeIndex, address, out Fixed64ByteBlock typed64Block);
        tempFixedByteBlock = typed64Block;
        break;   
      
      case 128:
        DiskBlockManager.ReadDataBlock(Fixed128ByteBlockTypeIndex, address, out Fixed128ByteBlock typed128Block);
        tempFixedByteBlock = typed128Block;
        break; 
      
      case 256:
        DiskBlockManager.ReadDataBlock(Fixed256ByteBlockTypeIndex, address, out Fixed256ByteBlock typed256Block);
        tempFixedByteBlock = typed256Block;
        break;
      
      case 512:
        DiskBlockManager.ReadDataBlock(Fixed512ByteBlockTypeIndex, address, out Fixed512ByteBlock typed512Block);
        tempFixedByteBlock = typed512Block;
        break;  
      
      case 1024:
        DiskBlockManager.ReadDataBlock(Fixed1KByteBlockTypeIndex, address, out Fixed1KByteBlock typed1KBlock);
        tempFixedByteBlock = typed1KBlock;
        break;        
      
      case 2048:
        DiskBlockManager.ReadDataBlock(Fixed2KByteBlockTypeIndex, address, out Fixed2KByteBlock typed2KBlock);
        tempFixedByteBlock = typed2KBlock;
        break; 
      
      case 4096:
        DiskBlockManager.ReadDataBlock(Fixed4KByteBlockTypeIndex, address, out Fixed4KByteBlock typed4KBlock);
        tempFixedByteBlock = typed4KBlock;
        break; 
      
      case 8192:
        DiskBlockManager.ReadDataBlock(Fixed8KByteBlockTypeIndex, address, out Fixed8KByteBlock typed8KBlock);
        tempFixedByteBlock = typed8KBlock;
        break;    
      
      case 16384:
        DiskBlockManager.ReadDataBlock(Fixed16KByteBlockTypeIndex, address, out Fixed16KByteBlock typed16KBlock);
        tempFixedByteBlock = typed16KBlock;
        break;  
      
      default: throw new ArgumentOutOfRangeException("Expected fixedByteBlock.Size to be a power of 2 from 16 to 16384");
    }
    
    fixedByteBlock.BytesStored = tempFixedByteBlock.BytesStored;
    fixedByteBlock.PreviousAddress = tempFixedByteBlock.PreviousAddress;
    fixedByteBlock.NextAddress = tempFixedByteBlock.NextAddress;
    Buffer.MemoryCopy(
      tempFixedByteBlock.DataPointer, 
      fixedByteBlock.DataPointer, 
      tempFixedByteBlock.Size, 
      fixedByteBlock.Size
    );
  }
  
  public void WriteFixedSizeBlock(long address, IFixedByteBlock fixedByteBlock)
  {
    switch (fixedByteBlock.Size)
    {
      case 16:
        {
          var typedBlock = (Fixed16ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed16ByteBlockTypeIndex, address, ref typedBlock);
          return;
        }
      
      case 32:
        {
          var typedBlock = (Fixed32ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed32ByteBlockTypeIndex, address, ref typedBlock);
          return;
        }  
      
      case 64:
        {
          var typedBlock = (Fixed64ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed64ByteBlockTypeIndex, address, ref typedBlock);
          return;
        }
      
      case 128:
        {
          var typedBlock = (Fixed128ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed128ByteBlockTypeIndex, address, ref typedBlock);
          return;
        } 
      
      case 256:
        {
          var typedBlock = (Fixed256ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed256ByteBlockTypeIndex, address, ref typedBlock);
          return;
        }
      
      case 512:
        {
          var typedBlock = (Fixed512ByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed512ByteBlockTypeIndex, address, ref typedBlock);
          return;
        } 
      
      case 1024:
        {
          var typedBlock = (Fixed1KByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed1KByteBlockTypeIndex, address, ref typedBlock);
          return;
        }  
      
      case 2048:
        {
          var typedBlock = (Fixed2KByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed2KByteBlockTypeIndex, address, ref typedBlock);
          return;
        } 
      
      case 4096:
        {
          var typedBlock = (Fixed4KByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed4KByteBlockTypeIndex, address, ref typedBlock);
          return;
        } 
      
      case 8192:
        {
          var typedBlock = (Fixed8KByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed8KByteBlockTypeIndex, address, ref typedBlock);
          return;
        }
      
      case 16384:
        {
          var typedBlock = (Fixed16KByteBlock) fixedByteBlock;
          DiskBlockManager.WriteDataBlock(Fixed16KByteBlockTypeIndex, address, ref typedBlock);
          return;
        }          
      
      default: throw new ArgumentOutOfRangeException("Expected fixedByteBlock.Size to be a power of 2 from 16 to 16384");
    }
  }  
}
