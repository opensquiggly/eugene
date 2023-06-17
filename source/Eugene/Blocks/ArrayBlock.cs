namespace Eugene.Blocks;

public struct ArrayBlock
{
  public short DataBlockTypeIndex { get; set; }
  public int DataSize { get; set; }
  public int MaxItems { get; set; }
  public int Count { get; set; }
  public long DataAddress { get; set; }
}
