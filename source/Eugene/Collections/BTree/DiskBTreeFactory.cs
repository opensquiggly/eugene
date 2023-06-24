namespace Eugene.Collections;

public class DiskBTreeFactory<TKey, TData>
  where TKey : struct, IComparable
  where TData : struct, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeFactory(DiskBTreeManager manager, short keyBlockTypeIndex, short dataBlockTypeIndex)
  {
    Manager = manager;
    KeyBlockTypeIndex = keyBlockTypeIndex;
    DataBlockTypeIndex = dataBlockTypeIndex;
    KeyArrayFactory = Manager.DiskBlockManager.ArrayManager.CreateFactory<TKey>(KeyBlockTypeIndex);
    DataArrayFactory = Manager.DiskBlockManager.ArrayManager.CreateFactory<TData>(DataBlockTypeIndex);
    NodeFactory = new DiskBTreeNodeFactory<TKey, TData>(this, keyBlockTypeIndex, dataBlockTypeIndex);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private short KeyBlockTypeIndex { get; }

  private short DataBlockTypeIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeManager Manager { get; }

  public DiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public short BTreeBlockTypeIndex => Manager.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => Manager.NodeBlockTypeIndex;

  public DiskArrayFactory<TKey> KeyArrayFactory { get; }

  public DiskArrayFactory<TData> DataArrayFactory { get; }

  public DiskBTreeNodeFactory<TKey, TData> NodeFactory { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree<TKey, TData> AppendNew()
  {
    // Create the root BTree node
    DiskBTreeNode<TKey, TData> rootNode = NodeFactory.AppendNew(true);

    // Create the BTree data block
    BTreeBlock btreeBlock = default;
    btreeBlock.KeyBlockTypeIndex = KeyBlockTypeIndex;
    btreeBlock.DataBlockTypeIndex = DataBlockTypeIndex;
    btreeBlock.Count = 0;
    btreeBlock.RootNodeAddress = rootNode.Address;
    long btreeAddress = DiskBlockManager.AppendDataBlock<BTreeBlock>(BTreeBlockTypeIndex, ref btreeBlock);

    return new DiskBTree<TKey, TData>(this, btreeAddress);
  }

  public DiskBTree<TKey, TData> LoadExisting(long address)
  {
    return new DiskBTree<TKey, TData>(this, address);
  }
}
