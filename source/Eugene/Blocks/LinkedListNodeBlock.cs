namespace Eugene.Blocks;

public struct LinkedListNodeBlock
{
  public long PreviousAddress { get; set; }
  public long NextAddress { get; set; }
  public long DataAddress { get; set; }
}
