using Eugene.Blocks;

namespace Eugene.Collections;

public class DiskLinkedList<TData> where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskLinkedList(DiskLinkedListFactory<TData> factory, long address)
  {
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskLinkedListFactory<TData> Factory { get; }

  public long Address { get; }

  public IDiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  public int LinkedListBlockTypeIndex => Factory.LinkedListBlockTypeIndex;

  public int LinkedListNodeBlockTypeIndex => Factory.LinkedListNodeBlockTypeIndex;

  public int DataBlockTypeIndex => Factory.DataBlockTypeIndex;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void AddLast(TData item)
  {
    DiskBlockManager.ReadDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, Address, out LinkedListBlock block);

    long dataAddress = DiskBlockManager.AppendDataBlock(DataBlockTypeIndex, ref item);
    LinkedListNodeBlock node = default;
    node.DataAddress = dataAddress;

    if (block.TailAddress == 0)
    {
      // List is Empty. Add first node.
      node.PreviousAddress = 0;
      node.NextAddress = 0;
      long nodeAddress = DiskBlockManager.AppendDataBlock(LinkedListNodeBlockTypeIndex, ref node);
      block.HeadAddress = nodeAddress;
      block.TailAddress = nodeAddress;
      block.Count++;
      DiskBlockManager.WriteDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, Address, ref block);
    }
    else
    {
      node.PreviousAddress = block.TailAddress;
      node.NextAddress = 0;
      long newNodeAddress = DiskBlockManager.AppendDataBlock(LinkedListNodeBlockTypeIndex, ref node);

      DiskBlockManager.ReadDataBlock<LinkedListNodeBlock>(LinkedListNodeBlockTypeIndex, block.TailAddress,
        out LinkedListNodeBlock tailNode);
      tailNode.NextAddress = newNodeAddress;
      DiskBlockManager.WriteDataBlock<LinkedListNodeBlock>(LinkedListNodeBlockTypeIndex, block.TailAddress, ref tailNode);
      block.TailAddress = newNodeAddress;
      block.Count++;
      DiskBlockManager.WriteDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, Address, ref block);
    }
  }

  public void InsertItemBefore(int index, TData item)
  {
    throw new NotImplementedException();
  }

  public void InsertItemAfter(int index, TData item)
  {
    throw new NotImplementedException();
  }
}
