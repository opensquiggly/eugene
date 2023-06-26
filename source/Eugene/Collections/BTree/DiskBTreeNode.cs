namespace Eugene.Collections;

public class DiskBTreeNode<TKey, TData>
  where TKey : struct, IComparable
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
  // Private Properties / Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private BTreeNodeBlock _nodeBlock = default;

  private BTreeNodeBlock NodeBlock => _nodeBlock;

  private bool IsLoaded { get; set; } = false;

  private DiskSortedArray<TKey> KeysArray { get; set; } = null;

  // DataArray holds an array of TData items when IsLeafNode is true,
  // or else is null if IsLeafNode is false
  private DiskArray<TData> DataArray { get; set; } = null;

  // ChildrenArray holds an array of addresses to child BTreeNodeBlocks if
  // IsLeafNode is false, or else is null if IsLeafNode is true
  private DiskArray<long> ChildrenArray { get; set; } = null;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree<TKey, TData> BTree { get; }

  public bool IsLeafNode => _nodeBlock.IsLeafNode == 1;

  public short BTreeBlockTypeIndex => NodeFactory.BTreeBlockTypeIndex;

  public short NodeBlockTypeIndex => NodeFactory.NodeBlockTypeIndex;

  public DiskBTreeNodeFactory<TKey, TData> NodeFactory { get; }

  public long Address { get; }

  public DiskBlockManager DiskBlockManager => NodeFactory.DiskBlockManager;

  public int NodeSize => BTree.NodeSize;

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

      DiskBTreeNode<TKey, TData> newNode = NodeFactory.AppendNew(this.BTree, this.IsLeafNode);
      newNode.EnsureLoaded();

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

  public void EnsureLoaded()
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
      else
      {
        this.InsertNonFull(key, data);
        return this;
      }
    }
    catch (Exception ex)
    {
      throw new Exception($"DiskBTreeNode.Insert Failed. key = {key}, data = {data}", ex);
    }
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

      // if (node.IsLeafNode)
      // {
      // Console.WriteLine($"Level = {level}");
      Console.WriteLine($"---------------");
      Console.WriteLine($"Address = {node.Address}");
      Console.WriteLine($"IsLeafNode = {node.IsLeafNode}");
      // }

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
