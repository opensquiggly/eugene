namespace Eugene.Collections;

public class DiskBTreeNode<TKey, TData> 
  where TKey : struct, IComparable
  where TData : struct, IComparable
{
  public const int NodeSize = 100;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeNode(DiskBTreeNodeFactory<TKey, TData> nodeFactory, long address)
  {
    NodeFactory = nodeFactory;
    Address = address;
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties / Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  private BTreeNodeBlock _nodeBlock = default;

  private BTreeNodeBlock NodeBlock => _nodeBlock;
  
  private bool IsLoaded { get; set; } = false;
  
  private DiskArray<TKey> KeysArray { get; set; } = null;

  // DataArray holds an array of TData items when IsLeafNode is true,
  // or else is null if IsLeafNode is false
  private DiskArray<TData> DataArray { get; set; } = null;

  // ChildrenArray holds an array of addresses to child BTreeNodeBlocks if
  // IsLeafNode is false, or else is null if IsLeafNode is true
  private DiskArray<long> ChildrenArray { get; set; } = null;
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public bool IsLeafNode => _nodeBlock.IsLeafNode == 1;
  
  public short BTreeBlockTypeIndex => NodeFactory.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => NodeFactory.NodeBlockTypeIndex;
  
  public DiskBTreeNodeFactory<TKey, TData> NodeFactory { get; }
  
  public long Address { get; }

  public bool IsFull => KeysArray.Count == NodeSize;
  
  public DiskBlockManager DiskBlockManager => NodeFactory.DiskBlockManager;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, Address, out _nodeBlock);
      KeysArray = NodeFactory.KeyArrayFactory.LoadExisting(_nodeBlock.KeysAddress);
      if (_nodeBlock.IsLeafNode == 1)
      {
        DataArray = NodeFactory.DataArrayFactory.LoadExisting(_nodeBlock.DataOrChildrenAddress);
        ChildrenArray = null;
      }
      else
      {
        ChildrenArray = DiskBlockManager.ArrayOfLongFactory.LoadExisting(_nodeBlock.DataOrChildrenAddress);
      }
      
      IsLoaded = true;
    } 
  }

  private void InsertNonFull(DiskBTreeNode<TKey, TData> node, TKey key, TData data)
  {
    EnsureLoaded();
    
    int i = node.KeysArray.Count;
    
    if (node.IsLeafNode)
    {
      while (i >= 1 && key.CompareTo(KeysArray[i - 1]) < 0)
      {
        // Note: The way DiskArray is currently implemented this will be slow because
        // it will generate a lot of disk reads & writes. We need a way to read and
        // write the whole array in one go, perhaps by adding a Shift() method to the
        // DiskArray class
        node.KeysArray[i] = node.KeysArray[i - 1];
        DataArray[i] = DataArray[i - 1];
      }
      node.KeysArray[i] = key;
      node.DataArray[i] = data;
    }
    else
    {
      while (i >= 1 && key.CompareTo(node.KeysArray[i - 1]) < 0)
      {
        i--;
      }
      i++;
      long childAddress = node.ChildrenArray[i - 1];
      DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(childAddress);
      if (childNode.IsFull)
      {
        childNode.Split(node, i - 1);
        if (key.CompareTo(node.KeysArray[i - 1]) > 0)
        {
          i++;
        }
      }
      InsertNonFull(childNode, key, data);
    }    
  }

  private void Split(DiskBTreeNode<TKey, TData> parentNode, int i)
  {
    EnsureLoaded();
    
    DiskBTreeNode<TKey, TData> newNode = NodeFactory.AppendNew(this.IsLeafNode);

    for (int j = 0; j < NodeSize / 2; j++)
    {
      newNode.KeysArray.AddItem(this.KeysArray[j + NodeSize / 2]);
    }

    if (this.IsLeafNode)
    {
      for (int j = 0; j < NodeSize / 2; j++)
      {
        newNode.DataArray.AddItem(this.DataArray[j + NodeSize / 2]);
      }      
    }
    else
    {
      for (int j = 0; j < NodeSize / 2; j++)
      {
        newNode.ChildrenArray.AddItem(this.ChildrenArray[j + NodeSize / 2]);
      }       
    }
    
    // Move the children of the parent node to make room for the new child
    for (int j = parentNode.KeysArray.Count; j >= i + 1; j--)
    {
      // Note: At this point parentNode is guaranteed not to be a leaf-node, so we
      // don't have to check whether we're dealing with the ChildrenArray or DataArray
      parentNode.KeysArray[j + 1] = parentNode.KeysArray[j];
      parentNode.ChildrenArray[j + 1] = parentNode.ChildrenArray[j];
    }

    parentNode.KeysArray[i] = this.KeysArray[NodeSize / 2];
    parentNode.ChildrenArray[i + 1] = newNode.Address;
    
    // TODO: Change the size of the arrays since we've moved half of them to the new node
    // this.KeysArray.Truncate(NodeSize / 2);
    // this.ChildrenArray.Truncate(NodeSize / 2);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeNode<TKey, TData> Insert(TKey key, TData data)
  {
    EnsureLoaded();
    
    if (IsFull)
    {
      DiskBTreeNode<TKey, TData> newRootNode = NodeFactory.AppendNew(false);
      newRootNode.EnsureLoaded();
      newRootNode.ChildrenArray.AddItem(this.Address);
      this.Split(newRootNode, 0);
      InsertNonFull(newRootNode, key, data);
      return newRootNode;
    }
    else
    {
      InsertNonFull(this, key, data);
      return this;
    }
  }
}
