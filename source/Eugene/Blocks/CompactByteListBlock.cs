namespace Eugene.Blocks;

public struct CompactByteListBlock
{
  public int Count { get; set; }
  public long HeadAddress { get; set; }
  public long TailAddress { get; set; }
}
