namespace Eugene.Blocks;

public unsafe struct Fixed256ByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public fixed byte Data[256];
  public int Size => 256;

  public byte* DataPointer
  {
    get
    {
      fixed (byte* ptr = &Data[0])
      {
        return ptr;
      }
    }
  }  
}
