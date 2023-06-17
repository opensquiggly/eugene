namespace Eugene.Blocks;

public struct LinkedListBlock
{
  public short DataBlockTypeIndex { get; set; }
  public short DataSize { get; set; }
  public int Count { get; set; }
  public long HeadAddress { get; set; }
  public long TailAddress { get; set; }
}
