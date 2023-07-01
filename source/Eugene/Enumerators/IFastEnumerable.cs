namespace Eugene.Enumerators;

public interface IFastEnumerable<TResult, TKey, TData> : IEnumerable<TKey>
  where TKey : IComparable<TKey>
  where TResult : IFastEnumerator<TKey, TData>
{
  TResult GetFastEnumerator();
}
