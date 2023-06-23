namespace Eugene.Collections;

public class DiskBTreeNode<TKey, TData>
  where TKey : struct, IComparable
  where TData : struct, IComparable
{
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

  public bool IsFull => KeysArray.Count == DiskBTree.NodeSize;

  public DiskBlockManager DiskBlockManager => NodeFactory.DiskBlockManager;

  public int NodeSize => DiskBTree.NodeSize;

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
    node.EnsureLoaded();

    int i = node.KeysArray.Count - 1;

    if (node.IsLeafNode)
    {
      while (i >= 0 && key.CompareTo(node.KeysArray[i]) < 0)
      {
        // Note: The way DiskArray is currently implemented this will be slow because
        // it will generate a lot of disk reads & writes. We need a way to read and
        // write the whole array in one go, perhaps by adding a Shift() method to the
        // DiskArray class
        node.KeysArray[i + 1] = node.KeysArray[i];
        node.DataArray[i + 1] = node.DataArray[i];
      }
      node.KeysArray[i + 1] = key;
      node.DataArray[i + 1] = data;
    }
    else
    {
      while (i >= 0 && key.CompareTo(node.KeysArray[i]) < 0)
      {
        i--;
      }
      i++;
      long childAddress = node.ChildrenArray[i];
      DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(childAddress);
      childNode.EnsureLoaded();
      if (childNode.KeysArray.Count == NodeSize)
      {
        childNode.Split(node, i);
        if (key.CompareTo(node.KeysArray[i - 1]) > 0)
        {
          i++;
        }
      }
      DiskBTreeNode<TKey, TData> targetNode = NodeFactory.LoadExisting(node.ChildrenArray[i]);
      InsertNonFull(targetNode, key, data);
    }
  }

  private void Split(DiskBTreeNode<TKey, TData> parentNode, int i)
  {
    EnsureLoaded();

    DiskBTreeNode<TKey, TData> newNode = NodeFactory.AppendNew(this.IsLeafNode);
    newNode.EnsureLoaded();
    
    this.KeysArray.AddItemsTo(newNode.KeysArray, NodeSize / 2);
    this.KeysArray.Truncate(NodeSize / 2);

    if (!this.IsLeafNode)
    {
      this.ChildrenArray.AddItemsTo(newNode.ChildrenArray, NodeSize / 2);
      this.ChildrenArray.Truncate(NodeSize / 2);  

    }
    else
    {
      this.DataArray.AddItemsTo(newNode.DataArray, NodeSize / 2);
      this.DataArray.Truncate(NodeSize / 2);    
    }

    // Move the children of the parent node to make room for the new child
    for (int j = parentNode.KeysArray.Count; j >= i + 1; j--)
    {
      // Note: At this point parentNode is guaranteed not to be a leaf-node, so we
      // don't have to check whether we're dealing with the ChildrenArray or DataArray
      parentNode.ChildrenArray[j + 1] = parentNode.ChildrenArray[j];
    }

    // parentNode.KeysArray[i + 1] = newNode.KeysArray[0];
    parentNode.ChildrenArray[i + 1] = newNode.Address;

    for (int j = parentNode.KeysArray.Count - 1; j >= i; j--)
    {
      parentNode.KeysArray[j + 1] = parentNode.KeysArray[j];
    }
    
    // parentNode.KeysArray[i] = this.KeysArray[(NodeSize / 2) - 1];
    parentNode.KeysArray[i] = this.KeysArray[(NodeSize / 2) - 1];
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TData Find(TKey key)
  {
    int i = 0;
    while (i < KeysArray.Count && key.CompareTo(KeysArray[i]) > 0)
    {
      i++;
    }

    if (IsLeafNode)
    {
      try
      {
        return DataArray[i];
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        throw;
      }
    }

    // Recurse into child node to find the leaf node
    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(ChildrenArray[i]);
    childNode.EnsureLoaded();
    return childNode.Find(key);
  }
  
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

  public void Print()
  {
    var queue = new Queue<long>();
    queue.Enqueue(this.Address);

    while (queue.Count > 0)
    {
      var nodeAddress = queue.Dequeue();
      var node = NodeFactory.LoadExisting(nodeAddress);
      node.EnsureLoaded();

      if (node.IsLeafNode)
      {
        // Console.WriteLine($"Level = {level}");
        Console.WriteLine($"---------------");
        Console.WriteLine($"Address = {node.Address}");
        Console.WriteLine($"IsLeafNode = {node.IsLeafNode}");
      }

      if (node.IsLeafNode)
      {
        Console.WriteLine($"Key Count = {node.KeysArray.Count}");
        Console.WriteLine($"Data Count = {node.DataArray.Count}");
        for (int x = 0; x < node.KeysArray.Count; x++)
        {
          Console.Write($"{node.KeysArray[x]}/{node.DataArray[x]} ");
        }

        Console.WriteLine();
      }
      else
      {
        // Console.WriteLine($"Key Count = {node.KeysArray.Count}");
        // Console.WriteLine($"Children Count = {node.ChildrenArray.Count}");
        // for (int x = 0; x < node.KeysArray.Count; x++)
        // {
        //   Console.Write($"{node.KeysArray[x]}/{node.ChildrenArray[x]} ");
        // }
        //
        // Console.WriteLine($" Overflow={node.ChildrenArray[^1]}");

        for (int x = 0; x < node.ChildrenArray.Count; x++)
        {
          queue.Enqueue(node.ChildrenArray[x]);
        }
      }
    }
  }
}
