namespace Eugene.Collections;

public class DiskBTreeCursor<TKey, TData> : IDictionaryCursor<TKey, TData>
  where TKey : struct, IComparable<TKey>
  where TData : struct
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTreeCursor(DiskBTree<TKey, TData> btree)
  {
    BTree = btree;
    Reset();
  }

  public DiskBTreeCursor(DiskBTree<TKey, TData> btree, DiskBTreeNode<TKey, TData> currentNode, int currentIndex)
  {
    BTree = btree;
    CurrentNode = currentNode;

    if (CurrentNode == null || CurrentNode.KeysCount == 0)
    {
      IsEmpty = true;
      CurrentIndex = -1;
    }

    CurrentIndex = currentIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool NavigatedPastBeginning { get; set; }

  private bool NavigatedPastEnd { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree<TKey, TData> BTree { get; private set; }

  public TData Current => CurrentData;

  object IEnumerator.Current => CurrentKey;

  public TData CurrentData
  {
    get
    {
      if (IsPastBeginning || IsPastEnd || CurrentIndex < 0 || CurrentIndex > CurrentNode.DataCount - 1)
      {
        throw new IndexOutOfRangeException();
      }

      return CurrentNode.GetDataAt(CurrentIndex);
    }
  }

  public int CurrentIndex { get; private set; }

  public TKey CurrentKey
  {
    get
    {
      if (IsPastBeginning || IsPastEnd || CurrentIndex < 0 || CurrentIndex > CurrentNode.DataCount - 1)
      {
        throw new IndexOutOfRangeException();
      }

      return CurrentNode.GetKeyAt(CurrentIndex);
    }
  }

  public DiskBTreeNode<TKey, TData> CurrentNode { get; private set; }

  public TData CurrentValue
  {
    get
    {
      if (IsPastBeginning || IsPastEnd || CurrentIndex < 0 || CurrentIndex > CurrentNode.DataCount - 1)
      {
        throw new IndexOutOfRangeException();
      }

      return CurrentNode.GetDataAt(CurrentIndex);
    }
  }

  public bool IsEmpty { get; private set; }

  public bool IsPastBeginning => IsEmpty || NavigatedPastBeginning;

  public bool IsPastEnd => IsEmpty || NavigatedPastEnd;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    if (NavigatedPastBeginning)
    {
      CurrentNode = BTree.GetFirstLeafNode();

      if (CurrentNode == null || CurrentNode.KeysCount == 0)
      {
        IsEmpty = true;
        CurrentIndex = -1;
        return false;
      }

      NavigatedPastBeginning = false;
      CurrentIndex = 0;
      return true;
    }

    if (IsPastEnd)
    {
      return false;
    }

    if (CurrentIndex < CurrentNode.DataCount - 1)
    {
      CurrentIndex++;
      return true;
    }

    if (CurrentNode.NextAddress != 0)
    {
      CurrentNode = CurrentNode.NodeFactory.LoadExisting(BTree, CurrentNode.NextAddress);
      CurrentNode.EnsureLoaded();
      CurrentIndex = 0;
      NavigatedPastEnd = CurrentNode.DataCount == 0;
      return true;
    }

    NavigatedPastEnd = true;
    return false;
  }

  public bool MovePrevious()
  {
    if (NavigatedPastEnd)
    {
      CurrentNode = BTree.GetLastLeafNode();

      if (CurrentNode == null || CurrentNode.KeysCount == 0)
      {
        IsEmpty = true;
        CurrentIndex = -1;
        return false;
      }

      NavigatedPastEnd = false;
      CurrentIndex = CurrentNode.KeysCount - 1;
      return true;
    }

    if (IsPastBeginning)
    {
      return false;
    }

    if (CurrentIndex > 0)
    {
      CurrentIndex--;
      return true;
    }

    if (CurrentNode.PreviousAddress != 0)
    {
      CurrentNode = CurrentNode.NodeFactory.LoadExisting(BTree, CurrentNode.PreviousAddress);
      CurrentNode.EnsureLoaded();
      CurrentIndex = CurrentNode.DataCount - 1;
      NavigatedPastBeginning = CurrentNode.DataCount == 0;
      return true;
    }

    NavigatedPastBeginning = true;
    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(TKey targetKey)
  {
    (DiskBTreeNode<TKey, TData> node, int index) = BTree.FindFirstGreaterThanOrEqual(targetKey);

    if (node == null)
    {
      NavigatedPastEnd = true;
      CurrentNode = null;
      CurrentIndex = -1;
      return false;
    }

    node.EnsureLoaded();
    NavigatedPastBeginning = false;
    NavigatedPastEnd = false;
    CurrentNode = node;
    CurrentIndex = index;
    return true;
  }

  public void Reset()
  {
    NavigatedPastBeginning = true;
    NavigatedPastEnd = false;
  }

  public void ResetToEnd()
  {
    NavigatedPastBeginning = false;
    NavigatedPastEnd = true;
  }
}
