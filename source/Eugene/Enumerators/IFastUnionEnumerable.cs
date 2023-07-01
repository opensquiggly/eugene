namespace Eugene.Enumerators;

public interface IFastUnionEnumerable<TKey, TData> : IFastEnumerable<IFastUnionEnumerator<TKey, TData>, TKey, TData>
  where TKey : IComparable<TKey>
{
}
