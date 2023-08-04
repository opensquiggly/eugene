namespace Eugene.Blocks;

public unsafe struct Fixed4KByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public fixed byte Data[4096];
  public int Size => 4096;

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
