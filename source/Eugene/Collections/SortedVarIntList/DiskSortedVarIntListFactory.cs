namespace Eugene.Collections;

public class DiskSortedVarIntListFactory
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntListFactory(DiskSortedVarIntListManager manager)
  {
    Manager = manager;
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  public DiskSortedVarIntListManager Manager { get; }

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public FixedByteBlockManager FixedByteBlockManager => Manager.FixedByteBlockManager;

  public DiskCompactByteListManager CompactByteListManager => Manager.CompactByteListManager;
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskSortedVarIntList AppendNew(ulong[] data = null)
  {
    DiskCompactByteList baseList = Manager.CompactByteListFactory.AppendNew();
    return new DiskSortedVarIntList(baseList, Manager.FixedByteBlockManager, this);
  }

  public void Delete()
  {
    throw new NotImplementedException();
  }

  public DiskCompactByteList LoadExisting(long address)
  {
    throw new NotImplementedException();
  }  
}

