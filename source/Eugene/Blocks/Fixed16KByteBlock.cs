namespace Eugene.Blocks;

public unsafe struct Fixed16KByteBlock : IFixedByteBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public ushort BytesStored { get; set; }
  public fixed byte Data[16384];
  public int Size => 16384;

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
