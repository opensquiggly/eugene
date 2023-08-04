namespace Eugene.Blocks;

public unsafe struct Fixed32ByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public fixed byte Data[32];
  public int Size => 32;

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
