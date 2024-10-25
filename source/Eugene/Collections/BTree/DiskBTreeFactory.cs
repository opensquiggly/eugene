namespace Eugene.Collections;

public class DiskBTreeFactory<TKey, TData>
  where TKey : struct, IComparable<TKey>
  where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeFactory(DiskBTreeManager manager, short keyBlockTypeIndex, short dataBlockTypeIndex)
  {
    Manager = manager;
    KeyBlockTypeIndex = keyBlockTypeIndex;
    DataBlockTypeIndex = dataBlockTypeIndex;
    KeyArrayFactory = Manager.DiskBlockManager.SortedArrayManager.CreateFactory<TKey>(KeyBlockTypeIndex);
    DataArrayFactory = Manager.DiskBlockManager.ArrayManager.CreateFactory<TData>(DataBlockTypeIndex);
    NodeFactory = new DiskBTreeNodeFactory<TKey, TData>(this, keyBlockTypeIndex, dataBlockTypeIndex);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public short KeyBlockTypeIndex { get; }

  public short DataBlockTypeIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeManager Manager { get; }

  public DiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public short BTreeBlockTypeIndex => Manager.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => Manager.NodeBlockTypeIndex;

  public DiskSortedArrayFactory<TKey> KeyArrayFactory { get; }

  public DiskArrayFactory<TData> DataArrayFactory { get; }

  public DiskBTreeNodeFactory<TKey, TData> NodeFactory { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public virtual DiskBTree<TKey, TData> AppendNew(short nodeSize)
  {
    // Create a placeholder for the BTree so we can pass the tree to
    // NodeFactory.AppendNew(). We do some fixups later.
    var result = new DiskBTree<TKey, TData>(this, 0, nodeSize);

    // Create the root BTree node
    DiskBTreeNode<TKey, TData> rootNode = NodeFactory.AppendNew(result, true);

    // Create the BTree data block
    BTreeBlock btreeBlock = default;
    btreeBlock.KeyBlockTypeIndex = KeyBlockTypeIndex;
    btreeBlock.DataBlockTypeIndex = DataBlockTypeIndex;
    btreeBlock.Count = 0;
    btreeBlock.NodeSize = nodeSize;
    btreeBlock.RootNodeAddress = rootNode.Address;
    long btreeAddress = DiskBlockManager.AppendDataBlock<BTreeBlock>(BTreeBlockTypeIndex, ref btreeBlock);

    result.FriendOfDiskBTreeFactory_ReplaceAddress(btreeAddress);

    return result;
  }

  public virtual DiskBTree<TKey, TData> LoadExisting(long address)
  {
    return new DiskBTree<TKey, TData>(this, address);
  }
}
