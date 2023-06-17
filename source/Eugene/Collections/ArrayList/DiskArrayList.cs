using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskArrayList<TRoot, TItem> 
  where TRoot : struct
  where TItem : struct
{
  public DiskArrayList(IDiskBlockManager diskBlockManager, int itemSize, int arrayListBlockType, int itemBlockType, long starterAddress)
  {
  }
  
  

  public int Count
  {
    get
    {
      // Return the current count
      return 0;
    }
  }

  public void Add(TItem item)
  {
    // ???
    // Find the starter node in the root record
    // Read the block
    // Check size of the node
    // if the number of items is less than the size of the node
    //   Add the item to the end of the list
    //   increment count by 1
    // else if count < max size allowed
    //   Expand the node by doubling the size or some other expansion factor
    //   Write the new expanded node
    //   Copy all the items from the old to the new location
    //   Add all the old items to the free list for that block type
    // else
    //   throw exception
    // end
  }

  public void Insert(int index, TItem item)
  {
    // Similar to add but shift items in the array to make space
  }

  public void ReadAt(int index, ref TItem item)
  {
  }
}
