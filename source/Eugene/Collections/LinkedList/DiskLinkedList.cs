namespace Eugene.Collections;

using System.Runtime.Versioning;

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

  public int Count
  {
    get
    {
      DiskBlockManager.ReadDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, Address, out LinkedListBlock block);
      return block.Count;
    }
  }

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

  public Position GetFirst()
  {
    DiskBlockManager.ReadDataBlock<LinkedListBlock>(LinkedListBlockTypeIndex, Address, out LinkedListBlock listBlock);

    if (listBlock.HeadAddress == 0)
    {
      return new Position(this, listBlock);
    }
    else
    {
      DiskBlockManager.ReadDataBlock<LinkedListNodeBlock>(LinkedListNodeBlockTypeIndex, listBlock.HeadAddress,
        out LinkedListNodeBlock nodeBlock);
      return new Position(listBlock.HeadAddress, this, listBlock, nodeBlock);
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

  public class Position
  {
    public Position(DiskLinkedList<TData> list, LinkedListBlock listBlock)
    {
      List = list;
      IsEmpty = true;
      NavigatedPastTail = false;
    }
    
    public Position(long nodeAddress, DiskLinkedList<TData> list, LinkedListBlock listBlock, LinkedListNodeBlock nodeBlock)
    {
      List = list;
      NodeAddress = nodeAddress;
      ListBlock = listBlock;
      NodeBlock = nodeBlock;
      IsEmpty = false;
      NavigatedPastTail = false;
    }
    
    private DiskLinkedList<TData> List { get; }
    
    private long NodeAddress { get; }

    private LinkedListBlock ListBlock { get; }
    
    private LinkedListNodeBlock NodeBlock { get; set; }
    
    private bool IsEmpty { get; }
    
    private bool NavigatedPastTail { get; set; }

    public TData Value
    {
      get
      {
        if (IsPastHead || IsPastTail)
        {
          throw new IndexOutOfRangeException();
        }

        List.DiskBlockManager.ReadDataBlock<TData>(List.DataBlockTypeIndex, NodeBlock.DataAddress, out TData val);
        return val;
      }
    }

    public bool IsPastHead
    {
      get
      {
        // ReSharper disable once ArrangeAccessorOwnerBody
        return IsEmpty;
      }
    }

    public bool IsPastTail
    {
      get
      {
        // ReSharper disable once ArrangeAccessorOwnerBody
        return IsEmpty || NavigatedPastTail;
      }
    }
    
    public void Next()
    {
      if (IsPastTail)
      {
        return;
      }

      if (NodeBlock.NextAddress == 0)
      {
        NavigatedPastTail = true;
        return;
      }
      
      List.DiskBlockManager.ReadDataBlock<LinkedListNodeBlock>(List.LinkedListNodeBlockTypeIndex, NodeBlock.NextAddress,
        out LinkedListNodeBlock newNodeBlock);
      NodeBlock = newNodeBlock;
    }

    public void Previous()
    {
    }
  }
}
