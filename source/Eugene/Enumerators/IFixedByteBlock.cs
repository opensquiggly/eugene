namespace Eugene.Enumerators;

public unsafe interface IFixedByteBlock
{
  public long PreviousAddress { get; set;}
  public long NextAddress { get; set; }
  public int Size { get; }
  public byte* DataPointer { get; }
}
