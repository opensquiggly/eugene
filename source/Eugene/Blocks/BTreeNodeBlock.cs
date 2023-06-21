namespace Eugene.Blocks;

public struct BTreeNodeBlock
{
  public short IsLeafNode { get; set; }
  public long KeysAddress { get; set; }
  
  // For leaf nodes, DataOrChildrenAddress holds the address of an array of data blocks
  // For non-leaf nodes, DataOrChildrenAddress holds the address of an array of BTreeNodeBlocks
  public long DataOrChildrenAddress { get; set; }
  
  public long NextAddress { get; set; }
  public long PreviousAddress { get; set; }  
}
