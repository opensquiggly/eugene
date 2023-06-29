namespace Eugene.Collections;

public class DiskBTreeNode<TKey, TData>
  where TKey : struct, IComparable<TKey>
  where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeNode(DiskBTree<TKey, TData> btree, DiskBTreeNodeFactory<TKey, TData> nodeFactory, long address)
  {
    BTree = btree;
    NodeFactory = nodeFactory;
    Address = address;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private BTreeNodeBlock _nodeBlock = default;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  // ChildrenArray holds an array of addresses to child BTreeNodeBlocks if
  // IsLeafNode is false, or else is null if IsLeafNode is true
  private DiskArray<long> ChildrenArray { get; set; } = null;

  // DataArray holds an array of TData items when IsLeafNode is true,
  // or else is null if IsLeafNode is false
  private DiskArray<TData> DataArray { get; set; } = null;

  private bool IsLoaded { get; set; } = false;

  private DiskSortedArray<TKey> KeysArray { get; set; } = null;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long Address { get; }

  public DiskBTree<TKey, TData> BTree { get; }

  public short BTreeBlockTypeIndex => NodeFactory.BTreeBlockTypeIndex;

  public int ChildrenCount
  {
    get
    {
      EnsureLoaded();
      return this.ChildrenArray.Count;
    }
  }

  public int ChildrenSize
  {
    get
    {
      EnsureLoaded();
      return this.ChildrenArray.MaxItems;
    }
  }

  public int DataCount
  {
    get
    {
      EnsureLoaded();
      return this.DataArray.Count;
    }
  }

  public long DataOrChildrenAddress
  {
    get
    {
      EnsureLoaded();
      return this.DataOrChildrenAddress;
    }
  }

  public DiskBlockManager DiskBlockManager => NodeFactory.DiskBlockManager;

  public bool IsLeafNode
  {
    get
    {
      EnsureLoaded();
      return _nodeBlock.IsLeafNode == 1;
    }
  }

  public long KeysAddress
  {
    get
    {
      EnsureLoaded();
      return _nodeBlock.KeysAddress;
    }
  }

  public int KeysCount => this.KeysArray.Count;

  public long NextAddress
  {
    get
    {
      EnsureLoaded();
      return _nodeBlock.NextAddress;
    }
  }

  public short NodeBlockTypeIndex => NodeFactory.NodeBlockTypeIndex;

  public DiskBTreeNodeFactory<TKey, TData> NodeFactory { get; }

  public int NodeSize => BTree.NodeSize;

  public long PreviousAddress
  {
    get
    {
      EnsureLoaded();
      return _nodeBlock.PreviousAddress;
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void InsertNonFull(TKey key, TData data)
  {
    int index = 0;

    try
    {
      this.EnsureLoaded();

      if (this.IsLeafNode)
      {
        index = this.KeysArray.AddItem(key);
        this.DataArray.InsertAt(index, data);
      }
      else
      {
        index = this.KeysArray.FindFirstGreaterThan(key);
        if (index == -1)
        {
          index = this.ChildrenArray.Count - 1;
        }
        long childAddress = this.ChildrenArray[index];
        DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(this.BTree, childAddress);
        childNode.EnsureLoaded();

        if (childNode.KeysArray.IsFull)
        {
          childNode.Split(this, index);

          // Since we just split the child node, we need to figure out which path
          // to go down
          index = this.KeysArray.FindFirstGreaterThan(key);
          if (index == -1)
          {
            index = this.ChildrenArray.Count - 1;
          }
          childAddress = this.ChildrenArray[index];
          childNode = NodeFactory.LoadExisting(this.BTree, childAddress);
          childNode.EnsureLoaded();
        }

        childNode.InsertNonFull(key, data);
      }
    }
    catch (Exception ex)
    {
      throw new Exception(
        $"DiskBTreeNode.InsertNonFull Failed. " +
        $"IsLeafNode = {this.IsLeafNode}, index = {index}, key = {key}, data = {data}", ex
      );
    }
  }

  private void Split(DiskBTreeNode<TKey, TData> parentNode, int i)
  {
    try
    {
      EnsureLoaded();

      // Create the new sibling node for the split
      DiskBTreeNode<TKey, TData> newNode = NodeFactory.AppendNew(this.BTree, this.IsLeafNode);
      newNode.EnsureLoaded();

      // Link the leaf nodes together
      if (this.IsLeafNode)
      {
        long oldNextAddress = this._nodeBlock.NextAddress;

        // Set this node's NextAddress to the new node
        this._nodeBlock.NextAddress = newNode.Address;
        DiskBlockManager.WriteDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, this.Address, ref this._nodeBlock);

        // Set the new node's NextAddress & PreviousAddress
        newNode._nodeBlock.PreviousAddress = this.Address;
        newNode._nodeBlock.NextAddress = oldNextAddress;
        DiskBlockManager.WriteDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, newNode.Address, ref newNode._nodeBlock);

        // Update the old next node's PreviousAddress to the new node
        if (oldNextAddress != 0)
        {
          DiskBTreeNode<TKey, TData> nextNode = NodeFactory.LoadExisting(BTree, oldNextAddress);
          nextNode.EnsureLoaded();
          nextNode._nodeBlock.PreviousAddress = newNode.Address;
          DiskBlockManager.WriteDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, nextNode.Address, ref nextNode._nodeBlock);
        }
      }

      int medianIndex = NodeSize / 2;
      TKey medianKey = this.KeysArray[medianIndex];

      if (!this.IsLeafNode)
      {
        this.KeysArray.AddItemsTo(newNode.KeysArray, medianIndex);
        this.KeysArray.Truncate(NodeSize / 2);
        this.ChildrenArray.AddItemsTo(newNode.ChildrenArray, medianIndex);
        this.ChildrenArray.Truncate(NodeSize / 2 + 1);
      }
      else
      {
        this.KeysArray.AddItemsTo(newNode.KeysArray, medianIndex);
        this.KeysArray.Truncate(NodeSize / 2);
        this.DataArray.AddItemsTo(newNode.DataArray, medianIndex);
        this.DataArray.Truncate(NodeSize / 2);
      }

      parentNode.KeysArray.Grow(1);
      parentNode.KeysArray.ShiftRight(i);
      parentNode.KeysArray[i] = medianKey;

      parentNode.ChildrenArray.Grow(1);
      parentNode.ChildrenArray.ShiftRight(i + 1);
      parentNode.ChildrenArray[i + 1] = newNode.Address;
    }
    catch (Exception ex)
    {
      throw new Exception($"DiskBTreeNode.Split Failed. Insert Index = {i}", ex);
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long GetChildAddressAt(int index)
  {
    EnsureLoaded();
    return this.ChildrenArray[index];
  }

  public TData GetDataAt(int index)
  {
    EnsureLoaded();
    return this.DataArray[index];
  }

  public TKey GetKeyAt(int index)
  {
    EnsureLoaded();
    return this.KeysArray[index];
  }

  public void EnsureLoaded()
  {
    if (!IsLoaded)
    {
      DiskBlockManager.ReadDataBlock<BTreeNodeBlock>(NodeBlockTypeIndex, Address, out _nodeBlock);
      KeysArray = NodeFactory.KeyArrayFactory.LoadExisting(_nodeBlock.KeysAddress);
      KeysArray.EnsureLoaded();
      if (_nodeBlock.IsLeafNode == 1)
      {
        DataArray = NodeFactory.DataArrayFactory.LoadExisting(_nodeBlock.DataOrChildrenAddress);
        DataArray.EnsureLoaded();
        ChildrenArray = null;
      }
      else
      {
        ChildrenArray = DiskBlockManager.ArrayOfLongFactory.LoadExisting(_nodeBlock.DataOrChildrenAddress);
        ChildrenArray.EnsureLoaded();
      }

      IsLoaded = true;
    }
  }

  public TData Find(TKey key)
  {
    int index;
    EnsureLoaded();

    if (IsLeafNode)
    {
      index = this.KeysArray.FindFirstEqual(key);
      if (index >= 0)
      {
        return DataArray[index];
      }

      throw new KeyNotFoundException($"The key {key} was not found in the BTree");
    }

    index = this.KeysArray.FindFirstGreaterThan(key);
    if (index == -1)
    {
      index = ChildrenArray.Count - 1;
    }

    // Recurse into child node to find the leaf node
    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[index]);
    childNode.EnsureLoaded();
    return childNode.Find(key);
  }

  public DiskBTreeCursor<TKey, TData> GetFirst()
  {
    EnsureLoaded();

    if (IsLeafNode)
    {
      return new DiskBTreeCursor<TKey, TData>(this.BTree, this, 0);
    }

    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[0]);
    childNode.EnsureLoaded();
    return childNode.GetFirst();
  }

  public DiskBTreeNode<TKey, TData> GetFirstLeafNode()
  {
    EnsureLoaded();

    if (IsLeafNode)
    {
      return this;
    }

    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[0]);
    childNode.EnsureLoaded();
    return childNode.GetFirstLeafNode();
  }

  public DiskBTreeCursor<TKey, TData> GetLast()
  {
    EnsureLoaded();

    if (IsLeafNode)
    {
      return new DiskBTreeCursor<TKey, TData>(this.BTree, this, this.DataCount - 1);
    }

    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[0]);
    childNode.EnsureLoaded();
    return childNode.GetLast();
  }

  public DiskBTreeNode<TKey, TData> GetLastLeafNode()
  {
    EnsureLoaded();

    if (IsLeafNode)
    {
      return this;
    }

    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[0]);
    childNode.EnsureLoaded();
    return childNode.GetFirstLeafNode();
  }

  public DiskBTreeNode<TKey, TData> Insert(TKey key, TData data)
  {
    try
    {
      EnsureLoaded();

      if (this.KeysArray.IsFull)
      {
        DiskBTreeNode<TKey, TData> newRootNode = NodeFactory.AppendNew(this.BTree, false);
        newRootNode.EnsureLoaded();
        newRootNode.ChildrenArray.AddItem(this.Address);
        this.Split(newRootNode, 0);
        newRootNode.InsertNonFull(key, data);
        return newRootNode;
      }

      this.InsertNonFull(key, data);
      return this;
    }
    catch (Exception ex)
    {
      throw new Exception($"DiskBTreeNode.Insert Failed. key = {key}, data = {data}", ex);
    }
  }

  public bool TryFind(TKey key, out TData data)
  {
    int index;
    EnsureLoaded();

    if (IsLeafNode)
    {
      index = this.KeysArray.FindFirstEqual(key);
      if (index >= 0)
      {
        data = DataArray[index];
        return true;
      }

      data = default;
      return false;
    }

    index = this.KeysArray.FindFirstGreaterThan(key);
    if (index == -1)
    {
      index = ChildrenArray.Count - 1;
    }

    // Recurse into child node to find the leaf node
    DiskBTreeNode<TKey, TData> childNode = NodeFactory.LoadExisting(BTree, ChildrenArray[index]);
    childNode.EnsureLoaded();
    return childNode.TryFind(key, out data);
  }

  public void Print()
  {
    var queue = new Queue<long>();
    queue.Enqueue(this.Address);

    while (queue.Count > 0)
    {
      long nodeAddress = queue.Dequeue();
      DiskBTreeNode<TKey, TData> node = NodeFactory.LoadExisting(this.BTree, nodeAddress);
      node.EnsureLoaded();

      Console.WriteLine($"---------------");
      Console.WriteLine($"Address = {node.Address}");
      Console.WriteLine($"IsLeafNode = {node.IsLeafNode}");

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
        Console.WriteLine($"Key Count = {node.KeysArray.Count}");
        Console.WriteLine($"Children Count = {node.ChildrenArray.Count}");
        for (int x = 0; x < node.KeysArray.Count; x++)
        {
          Console.Write($"{node.KeysArray[x]}/{node.ChildrenArray[x]} ");
        }

        Console.WriteLine($" Overflow={node.ChildrenArray[^1]}");

        for (int x = 0; x < node.ChildrenArray.Count; x++)
        {
          queue.Enqueue(node.ChildrenArray[x]);
        }
      }
    }
  }
}
