namespace Eugene.Blocks;

public unsafe struct Fixed1KByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public ushort BytesStored { get; set; }
  public fixed byte Data[1024];
  public int Size => 1024;

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
