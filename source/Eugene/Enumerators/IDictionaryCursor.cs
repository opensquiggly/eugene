namespace Eugene.Enumerators;

public interface IDictionaryCursor<TKey, TData> : IFastEnumerator<TKey, TData> where TKey : IComparable<TKey>
{
  int CurrentIndex { get; }
  TData CurrentValue { get; }
  bool IsEmpty { get; }
  bool IsPastBeginning { get; }
  bool IsPastEnd { get; }
  bool MovePrevious();
  void ResetToEnd();
}
