// using System.Collections;
// using Eugene.Enumerators;
//
// namespace Eugene.Collections;
//
// public class DiskBTreeFastEnumerator<TKey, TData> : IFastEnumerator<TKey, TData> 
//   where TKey : struct, IComparable<TKey>
//   where TData : struct
// {
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   // Constructors
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//
//   public DiskBTreeFastEnumerator(DiskBTree<TKey, TData> btree)
//   {
//     BTree = btree;
//     Reset();
//   }
//   
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   // Private Properties
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   
//   private DiskBTree<TKey, TData> BTree { get; }
//   
//   private DiskBTreeNode<TKey, TData> CurrentNode { get; private set; }
//
//   private int CurrentIndex { get; private set; }
//
//   private bool IsEmpty { get; }
//
//   private bool NavigatedPastTail { get; private set; }
//
//   private bool IsPastHead => IsEmpty;
//
//   private bool IsPastTail => IsEmpty || NavigatedPastTail;
//   
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   // Explicit IEnumerator Properties
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//
//   object IEnumerator.Current => CurrentData;
//
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   // Public Properties
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//
//   public TData CurrentData
//   {
//     get
//     {
//       if (IsPastHead || IsPastTail)
//       {
//         throw new IndexOutOfRangeException();
//       }
//
//       return CurrentNode.GetDataAt(CurrentIndex);
//     }
//   }
//   
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   // Public Methods
//   // /////////////////////////////////////////////////////////////////////////////////////////////
//   
//   public void Dispose()
//   {
//   }
//   
//   public bool MoveNext()
//   {
//     if (IsPastTail)
//     {
//       return;
//     }
//
//     if (CurrentIndex < CurrentNode.DataCount - 1)
//     {
//       CurrentIndex++;
//       return;
//     }
//
//     if (CurrentNode.NextAddress != 0)
//     {
//       CurrentNode = CurrentNode.NodeFactory.LoadExisting(BTree, CurrentNode.NextAddress);
//       CurrentNode.EnsureLoaded();
//       CurrentIndex = 0;
//       NavigatedPastTail = CurrentNode.DataArray.Count == 0;
//       return;
//     }
//
//     NavigatedPastTail = true;
//   }
//
//   public bool MoveUntilGreaterThanOrEqual(TKey target)
//   {
//     throw new NotImplementedException();
//   }
//
//   public void Reset()
//   {
//     CurrentNode = btree.GetFirstLeafNode();
//     CurrentIndex = 0;
//     IsEmpty = false;
//     NavigatedPastTail = false;
//   }
// }
