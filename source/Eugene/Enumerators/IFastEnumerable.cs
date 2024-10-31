namespace Eugene.Enumerators;

public interface IFastEnumerable<TResult, TKey, TData> : IEnumerable<TData>
  where TKey : IComparable<TKey>
  where TResult : IFastEnumerator<TKey, TData>
{
  TResult GetFastEnumerator();
}
