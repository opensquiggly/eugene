namespace Eugene.Collections;

public static class DiskBTree
{
  public const int NodeSize = 100;
}

public class DiskBTree<TKey, TData>
  where TKey : struct, IComparable
  where TData : struct, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree(DiskBTreeFactory<TKey, TData> factory, long address)
  {
    Factory = factory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties / Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private BTreeBlock _btreeBlock = default;

  private bool IsLoaded { get; set; } = false;

  private BTreeBlock BTreeBlock => _btreeBlock;

  private DiskBTreeNode<TKey, TData> RootNode { get; set; } = null;

  private DiskBlockManager DiskBlockManager => Factory.DiskBlockManager;

  private DiskBTreeNodeFactory<TKey, TData> NodeFactory => Factory.NodeFactory;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public short BTreeBlockTypeIndex => Factory.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => Factory.NodeBlockTypeIndex;

  public DiskBTreeFactory<TKey, TData> Factory { get; }

  public DiskArrayFactory<TKey> KeyArrayFactory => Factory.KeyArrayFactory;

  public DiskArrayFactory<TData> DataArrayFactory => Factory.DataArrayFactory;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<BTreeBlock>(BTreeBlockTypeIndex, Address, out _btreeBlock);
      RootNode = NodeFactory.LoadExisting(_btreeBlock.RootNodeAddress);
      RootNode.EnsureLoaded();
      IsLoaded = true;
    }
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
    catch (Exception)
    {
      return default(TData);
    }
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
        _btreeBlock.RootNodeAddress = newRootNode.Address;
        DiskBlockManager.WriteDataBlock<BTreeBlock>(BTreeBlockTypeIndex, Address, ref _btreeBlock);
        RootNode = newRootNode;
      }

      return true;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      return false;
      // throw;
    }
  }

  public void Print()
  {
    EnsureLoaded();
    RootNode.Print();
  }
}
