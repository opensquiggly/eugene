namespace Eugene.Collections;

using System.Collections;
using Enumerators;

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

  public TKey Current => CurrentKey;

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

  public bool MoveUntilGreaterThanOrEqual(TKey target)
  {
    throw new NotImplementedException();
  }

  public void Reset()
  {
    CurrentNode = BTree.GetFirstLeafNode();

    if (CurrentNode == null || CurrentNode.KeysCount == 0)
    {
      IsEmpty = true;
      CurrentIndex = -1;
    }

    CurrentIndex = 0;
  }

  public void ResetToEnd()
  {
    CurrentNode = BTree.GetLastLeafNode();

    if (CurrentNode == null || CurrentNode.KeysCount == 0)
    {
      IsEmpty = true;
      CurrentIndex = -1;
    }

    CurrentIndex = CurrentNode.KeysCount - 1;
  }
}
