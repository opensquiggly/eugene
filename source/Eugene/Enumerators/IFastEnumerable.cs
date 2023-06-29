namespace Eugene.Enumerators;

public interface IFastEnumerable<TKey, TData> : IEnumerable<TData> where TKey : IComparable<TKey>
{
  IFastEnumerator<TKey, TData> GetFastEnumerator();
}
