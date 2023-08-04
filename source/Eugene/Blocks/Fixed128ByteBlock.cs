namespace Eugene.Blocks;

public unsafe struct Fixed128ByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public fixed byte Data[128];
  public int Size => 128;

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
