namespace Eugene.Blocks;

public struct BlockMetadataBlock
{
  public short BlockTypeId { get; set; }
  public short Free { get; set; }
  public long NextBlock { get; set; }
}
