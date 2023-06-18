using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskLinkedListFactory<TData> where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskLinkedListFactory(DiskLinkedListManager manager, int dataBlockTypeIndex)
  {
    Manager = manager;
    DataBlockTypeIndex = dataBlockTypeIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskLinkedListManager Manager { get; }

  public IDiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public int DataBlockTypeIndex { get; }

  public int LinkedListBlockTypeIndex => Manager.LinkedListBlockTypeIndex;

  public int LinkedListNodeBlockTypeIndex => Manager.LinkedListNodeBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskLinkedList<TData> AppendNew()
  {
    LinkedListBlock block = default;
    block.HeadAddress = 0;
    block.TailAddress = 0;
    block.DataBlockTypeIndex = (short) DataBlockTypeIndex;
    block.Count = 0;
    // TODO:
    // block.DataSize = ???; Where do I get this from?
    long address = DiskBlockManager.AppendDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, ref block);

    return new DiskLinkedList<TData>(this, address);
  }

  public DiskLinkedList<TData> LoadExisting(long address)
  {
    return new DiskLinkedList<TData>(this, address);
  }
}
