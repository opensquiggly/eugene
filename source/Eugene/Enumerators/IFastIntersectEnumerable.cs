namespace Eugene.Enumerators;

public interface IFastIntersectEnumerable<TKey, TData> : IFastEnumerable<IFastIntersectEnumerator<TKey, TData>, TKey, TData>
  where TKey : IComparable<TKey>
{
}
