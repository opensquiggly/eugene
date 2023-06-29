namespace Eugene.Enumerators;

public interface IFastEnumerable<TKey, TData> : IEnumerable<TKey> where TKey : IComparable<TKey>
{
  IFastEnumerator<TKey, TData> GetFastEnumerator();
}
