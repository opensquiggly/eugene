namespace Eugene.Collections;

public class DiskSortedVarIntListManager
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntListManager(
    IDiskBlockManager diskBlockManager, 
    FixedByteBlockManager fixedByteBlockManager,
    DiskCompactByteListManager compactByteListManager,
    DiskCompactByteListFactory compactByteListFactory
  )
  {
    DiskBlockManager = diskBlockManager;
    FixedByteBlockManager = fixedByteBlockManager;
    CompactByteListManager = compactByteListManager;
    CompactByteListFactory = compactByteListFactory;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager { get; }
  
  public FixedByteBlockManager FixedByteBlockManager { get; }
  
  public DiskCompactByteListManager CompactByteListManager { get; }
  
  public DiskCompactByteListFactory CompactByteListFactory { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntListFactory CreateFactory()
  {
    return new DiskSortedVarIntListFactory(this);
  }  
}
