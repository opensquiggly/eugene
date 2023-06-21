namespace Eugene.Collections;

public class DiskBTreeNodeFactory<TKey, TData>
  where TKey : struct, IComparable
  where TData : struct, IComparable
{
  public const int NodeSize = 100;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeNodeFactory(DiskBTreeFactory<TKey, TData> btreeFactory, short keyBlockTypeIndex, short dataBlockTypeIndex)
  {
    BTreeFactory = btreeFactory;
    KeyBlockTypeIndex = keyBlockTypeIndex;
    DataBlockTypeIndex = dataBlockTypeIndex;
    KeyArrayFactory = Manager.DiskBlockManager.ArrayManager.CreateFactory<TKey>(KeyBlockTypeIndex);
    DataArrayFactory = Manager.DiskBlockManager.ArrayManager.CreateFactory<TData>(DataBlockTypeIndex);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private short KeyBlockTypeIndex { get; }

  private short DataBlockTypeIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeFactory<TKey, TData> BTreeFactory { get; }

  public DiskBTreeManager Manager => BTreeFactory.Manager;

  public DiskBlockManager DiskBlockManager => Manager.DiskBlockManager;

  public short BTreeBlockTypeIndex => Manager.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => Manager.NodeBlockTypeIndex;

  public DiskArrayFactory<TKey> KeyArrayFactory { get; }

  public DiskArrayFactory<TData> DataArrayFactory { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeNode<TKey, TData> AppendNew(bool isLeafNode)
  {
    long dataOrChildrenAddress;

    // Create the keys array
    DiskArray<TKey> keysArray = KeyArrayFactory.AppendNew(NodeSize);

    if (isLeafNode)
    {
      // For leaf nodes, create the array that holds the data
      DiskArray<TData> dataArray = DataArrayFactory.AppendNew(NodeSize);
      dataOrChildrenAddress = dataArray.Address;
    }
    else
    {
      // For non-leaf nodes, create the array that holds the child nodes of the btree
      DiskArray<long> childrenArray = DiskBlockManager.ArrayOfLongFactory.AppendNew(NodeSize);
      dataOrChildrenAddress = childrenArray.Address;
    }

    // Create the node block
    BTreeNodeBlock nodeBlock = default;
    nodeBlock.IsLeafNode = (short) (isLeafNode ? 1 : 0);
    nodeBlock.KeysAddress = keysArray.Address;
    nodeBlock.DataOrChildrenAddress = dataOrChildrenAddress;
    nodeBlock.NextAddress = 0;
    nodeBlock.PreviousAddress = 0;
    long nodeAddress = DiskBlockManager.AppendDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, ref nodeBlock);

    return new DiskBTreeNode<TKey, TData>(this, nodeAddress);
  }

  public DiskBTreeNode<TKey, TData> LoadExisting(long address)
  {
    return new DiskBTreeNode<TKey, TData>(this, address);
  }
}
