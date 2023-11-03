namespace Eugene.Blocks;

public unsafe struct Fixed8KByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public ushort BytesStored { get; set; }
  public fixed byte Data[8192];
  public int Size => 8192;

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
