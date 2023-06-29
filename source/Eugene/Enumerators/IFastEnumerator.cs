namespace Eugene.Enumerators;

public interface IFastEnumerator<TKey, TData> : IEnumerator<TKey> where TKey : IComparable<TKey>
{
  public TKey CurrentKey { get; }
  public TData CurrentData { get; }
  public bool MoveUntilGreaterThanOrEqual(TKey target);
}
