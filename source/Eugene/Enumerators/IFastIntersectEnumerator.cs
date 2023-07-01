namespace Eugene.Enumerators;

public interface IFastIntersectEnumerator<TKey, TData> : IFastEnumerator<TKey, TData> where TKey : IComparable<TKey>
{
  public TData CurrentData1 { get; }
  public TData CurrentData2 { get; }
}
