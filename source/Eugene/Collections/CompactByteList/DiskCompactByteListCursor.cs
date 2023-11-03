namespace Eugene.Collections;

public class DiskCompactByteListCursor : IEnumerator<byte>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteListCursor(DiskCompactByteList list)
  {
    List = list;
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool NavigatedPastBeginning { get; set; }

  private bool NavigatedPastEnd { get; set; }

  private unsafe byte CurrentItem => CurrentBlock.DataPointer[CurrentIndex];

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskCompactByteList List { get; private set; }

  unsafe byte IEnumerator<byte>.Current => this.CurrentItem;

  public int CurrentIndex { get; private set; }

  public IFixedByteBlock CurrentBlock { get; set; }

  public object Current => this.CurrentItem;

  public bool IsEmpty => false; // TODO: Implement this

  public bool IsPastBeginning => IsEmpty || NavigatedPastBeginning;

  public bool IsPastEnd => IsEmpty || NavigatedPastEnd;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    List.EnsureLoaded();

    if (NavigatedPastBeginning)
    {
      CurrentBlock = List.ReadHeadBlock();
      NavigatedPastBeginning = false;
      NavigatedPastEnd = CurrentBlock.BytesStored == 0;
      CurrentIndex = 0;
      return CurrentBlock.BytesStored > 0;
    }

    if (IsPastEnd)
    {
      return false;
    }

    if (CurrentIndex < CurrentBlock.BytesStored - 1)
    {
      CurrentIndex++;
      return true;
    }

    if (CurrentBlock.NextAddress != 0)
    {
      CurrentBlock = List.ReadBlock(CurrentBlock.NextAddress);
      NavigatedPastEnd = CurrentBlock.BytesStored == 0;
      CurrentIndex = 0;
      return CurrentBlock.BytesStored > 0;
    }

    NavigatedPastEnd = true;
    return false;
  }

  public void Reset()
  {
    CurrentBlock = null;
    CurrentIndex = -1;
    NavigatedPastBeginning = true;
    NavigatedPastEnd = false;
  }

  public void ResetToEnd()
  {
    NavigatedPastBeginning = false;
    NavigatedPastEnd = true;
  }
}
