namespace Eugene.Collections;

public interface IDiskBTreeCursor<TKey, TData> where TKey : struct, IComparable<TKey> where TData : struct
{
  DiskBTree<TKey, TData> BTree { get; }
  TData Current { get; }
  TData CurrentData { get; }
  int CurrentIndex { get; }
  TKey CurrentKey { get; }
  DiskBTreeNode<TKey, TData> CurrentNode { get; }
  TData CurrentValue { get; }
  bool IsEmpty { get; }
  bool IsPastBeginning { get; }
  bool IsPastEnd { get; }
  void Dispose();
  bool MoveNext();
  bool MovePrevious();
  bool MoveUntilGreaterThanOrEqual(TKey target);
  void Reset();
  void ResetToEnd();
}
