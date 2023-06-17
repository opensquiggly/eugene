using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskImmutableStringManager
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableStringManager(IDiskBlockManager diskBlockManager, int arrayBlockTypeIndex)
  {
    DiskBlockManager = diskBlockManager;
    ArrayBlockTypeIndex = arrayBlockTypeIndex;
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public IDiskBlockManager DiskBlockManager { get; }

  public int ArrayBlockTypeIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskImmutableStringFactory CreateFactory(short dataBlockTypeIndex)
  {
    return new DiskImmutableStringFactory(this, dataBlockTypeIndex);
  }
}