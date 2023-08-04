namespace Eugene.Blocks;

public unsafe struct Fixed2KByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public fixed byte Data[2048];
  public int Size => 2048;

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
