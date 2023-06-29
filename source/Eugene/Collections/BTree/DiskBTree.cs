namespace Eugene.Collections;

using System.Collections;
using Enumerators;

public class DiskBTree<TKey, TData> : IFastEnumerable<TKey, TData>
  where TKey : struct, IComparable<TKey>
  where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree(DiskBTreeFactory<TKey, TData> factory, long address, short tempNodeSize = 0)
  {
    Factory = factory;
    Address = address;

    // The assumed node size to use until the BTree has been loaded from disk
    // and the node size is read from the associated BTreeBlock
    TempNodeSize = tempNodeSize;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private BTreeBlock _bTreeBlock = default;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool IsLoaded { get; set; } = false;

  private DiskBTreeNode<TKey, TData> RootNode { get; set; } = null;

  private DiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  private DiskBTreeNodeFactory<TKey, TData> NodeFactory => Factory.NodeFactory;

  private short TempNodeSize { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; private set; }

  public short BTreeBlockTypeIndex => Factory.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => Factory.NodeBlockTypeIndex;

  public DiskBTreeFactory<TKey, TData> Factory { get; }

  public DiskSortedArrayFactory<TKey> KeyArrayFactory => Factory.KeyArrayFactory;

  public DiskArrayFactory<TData> DataArrayFactory => Factory.DataArrayFactory;

  public short NodeSize => IsLoaded ? _bTreeBlock.NodeSize : TempNodeSize;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<BTreeBlock>(BTreeBlockTypeIndex, Address, out _bTreeBlock);
      RootNode = NodeFactory.LoadExisting(this, _bTreeBlock.RootNodeAddress);
      RootNode.EnsureLoaded();
      IsLoaded = true;
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Helper Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void FriendOfDiskBTreeFactory_ReplaceAddress(long address)
  {
    // This function should not normally be used by the client. It is a helper function
    // used by DiskBTreeFactory in the AppendNew() function.

    if (IsLoaded)
    {
      throw new Exception("Cannot replace the address after the BTree has been loaded");
    }

    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TData Find(TKey key)
  {
    try
    {
      EnsureLoaded();
      return RootNode.Find(key);
    }
    catch (Exception ex)
    {
      throw new Exception($"DiskBTree.Find Failed. key = {key}", ex);
    }
  }

  public IEnumerator<TData> GetEnumerator()
  {
    return new DiskBTreeCursor<TKey, TData>(this);
  }

  IEnumerator IEnumerable.GetEnumerator()
  {
    return GetEnumerator();
  }

  public IFastEnumerator<TKey, TData> GetFastEnumerator()
  {
    return new DiskBTreeCursor<TKey, TData>(this);
  }

  public DiskBTreeCursor<TKey, TData> GetFirst()
  {
    EnsureLoaded();
    return RootNode.GetFirst();
  }

  public DiskBTreeNode<TKey, TData> GetFirstLeafNode()
  {
    EnsureLoaded();
    return RootNode.GetFirstLeafNode();
  }

  public DiskBTreeCursor<TKey, TData> GetLast()
  {
    EnsureLoaded();
    return RootNode.GetLast();
  }

  public DiskBTreeNode<TKey, TData> GetLastLeafNode()
  {
    EnsureLoaded();
    return RootNode.GetLastLeafNode();
  }

  public bool Insert(TKey key, TData data)
  {
    try
    {
      EnsureLoaded();
      DiskBTreeNode<TKey, TData> newRootNode = RootNode.Insert(key, data);
      if (newRootNode.Address != RootNode.Address)
      {
        // Root node has changed
        _bTreeBlock.RootNodeAddress = newRootNode.Address;
        DiskBlockManager.WriteDataBlock<BTreeBlock>(BTreeBlockTypeIndex, Address, ref _bTreeBlock);
        RootNode = newRootNode;
      }

      return true;
    }
    catch (Exception ex)
    {
      throw new Exception($"DiskBTree.Insert Failed. key = {key}, data = {data}", ex);
    }
  }

  public void Print()
  {
    EnsureLoaded();
    RootNode.Print();
  }

  public bool TryFind(TKey key, out TData data)
  {
    EnsureLoaded();
    return RootNode.TryFind(key, out data);
  }
}
