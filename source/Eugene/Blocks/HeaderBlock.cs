namespace Eugene.Blocks;

public struct HeaderBlock
{
  public int HeaderBlockSize { get; set; }
  public int ClientHeaderBlockSize { get; set; }
  public short SchemaVersion { get; set; }
  public int BlockTypesCount { get; set; }
  public int Data1 { get; set; }
  public int Data2 { get; set; }
  public int Data3 { get; set; }
  public int Data4 { get; set; }
  public long Address1 { get; set; }
  public long Address2 { get; set; }
  public long Address3 { get; set; }
  public long Address4 { get; set; }
  public long Address5 { get; set; }
  public long Address6 { get; set; }
  public long Address7 { get; set; }
  public long Address8 { get; set; }
}
