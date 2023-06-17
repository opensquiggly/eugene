namespace Eugene.Blocks;

public struct BlockTypeMetadataBlock
{
  public int ItemSize { get; set; }
  public long FreeListHeadNode { get; set; }
}
