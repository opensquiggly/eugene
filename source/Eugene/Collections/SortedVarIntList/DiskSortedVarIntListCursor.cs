namespace Eugene.Collections;

public class DiskSortedVarIntListCursor : IEnumerator<ulong>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntListCursor(DiskSortedVarIntList list)
  {
    List = list;
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskSortedVarIntList List { get; }

  private bool NavigatedPastBeginning { get; set; }

  private bool NavigatedPastEnd { get; set; }

  private ulong LastValue { get; set; }

  private ulong CurrentDiffValue { get; set; }

  private ulong CurrentValue => LastValue + CurrentDiffValue;

  private int CurrentIndex { get; set; }

  private IFixedByteBlock CurrentBlock { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  unsafe ulong IEnumerator<ulong>.Current => this.CurrentValue;

  public object Current => this.CurrentValue;

  public bool IsEmpty => false; // TODO: Implement this

  public bool IsPastBeginning => IsEmpty || NavigatedPastBeginning;

  public bool IsPastEnd => IsEmpty || NavigatedPastEnd;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private unsafe bool GetNextByteAndAdvanceIndex(out byte result)
  {
    if (CurrentBlock == null || CurrentBlock.BytesStored == 0)
    {
      result = 0;
      return false;
    }

    result = (CurrentIndex <= CurrentBlock.BytesStored - 1) ? CurrentBlock.DataPointer[CurrentIndex] : (byte) 0;

    if (CurrentIndex < CurrentBlock.BytesStored - 1)
    {
      CurrentIndex++;
      return true;
    }

    if (CurrentBlock.NextAddress == 0)
    {
      CurrentBlock = null;
      return true;
    }

    CurrentBlock = List.ReadBlock(CurrentBlock.NextAddress);
    CurrentIndex = 0;
    return true;
  }

  private bool GetNextValueAndAdvanceIndex(out ulong result)
  {
    result = 0;
    int shift = 0;

    while (GetNextByteAndAdvanceIndex(out byte b))
    {
      result |= (uint) ((b & 0x7F) << shift);  // Take the 7 data bits and shift them into position
      if ((b & 0x80) == 0)  // If the continuation bit is not set, we are done
      {
        return true;
      }
      shift += 7;
    }

    return false;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    bool hasValue;
    ulong result;

    List.EnsureLoaded();

    if (NavigatedPastBeginning)
    {
      CurrentBlock = List.ReadHeadBlock();
      LastValue = 0;
      CurrentIndex = 0;
      hasValue = GetNextValueAndAdvanceIndex(out result);
      CurrentDiffValue = result;
      NavigatedPastBeginning = false;
      return hasValue;
    }

    if (IsPastEnd)
    {
      return false;
    }

    LastValue = CurrentValue;
    hasValue = GetNextValueAndAdvanceIndex(out result);
    CurrentDiffValue = result;

    if (!hasValue)
    {
      NavigatedPastEnd = true;
    }

    return hasValue;
  }

  public void Reset()
  {
    CurrentBlock = null;
    CurrentIndex = -1;
    NavigatedPastBeginning = true;
    NavigatedPastEnd = false;
    LastValue = 0;
    CurrentDiffValue = 0;
  }

  public void ResetToEnd()
  {
    NavigatedPastBeginning = false;
    NavigatedPastEnd = true;
  }
}
