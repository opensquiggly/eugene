namespace Eugene.Blocks;

public struct BTreeBlock
{
  public short KeyBlockTypeIndex { get; set; }
  public short DataBlockTypeIndex { get; set; }
  public int Count { get; set; }
  public short NodeSize { get; set; }
  public long RootNodeAddress { get; set; }
}
