namespace Eugene.Blocks;

public struct BTreeLeafNodeBlock
{
  public long KeysAddress { get; set; }
  public long DataAddress { get; set; }
  public long NextAddress { get; set; }
  public long PreviousAddress { get; set; }
}
