namespace Eugene.Helpers;

public class FixedByteBlock
{
  public static IFixedByteBlock CreateFixedByteBlock(int size)
  {
    switch (size)
    {
      case 16: return new Fixed16ByteBlock();
      case 32: return new Fixed32ByteBlock();
      case 64: return new Fixed64ByteBlock();
      case 128: return new Fixed128ByteBlock();
      case 256: return new Fixed256ByteBlock();
      case 512: return new Fixed512ByteBlock();
      case 1024: return new Fixed1KByteBlock();
      case 2048: return new Fixed2KByteBlock();
      case 4096: return new Fixed4KByteBlock();
      case 8192: return new Fixed8KByteBlock();
      case 16384: return new Fixed16KByteBlock();
      default: throw new ArgumentOutOfRangeException("Expected a power of 2 from 16 to 16384");
    }
  }
}
