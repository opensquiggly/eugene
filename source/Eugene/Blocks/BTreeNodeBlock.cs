namespace Eugene.Blocks;

public struct BTreeNodeBlock
{
  public short IsLeafNode { get; set; }
  public long KeysAddress { get; set; }
  public long DataAddress { get; set; }
  public long NextAddress { get; set; }
  public long PreviousAddress { get; set; }  
}
