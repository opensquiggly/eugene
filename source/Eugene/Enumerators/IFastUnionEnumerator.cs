namespace Eugene.Enumerators;

public interface IFastUnionEnumerator<TKey, TData> : IFastEnumerator<TKey, TData> where TKey : IComparable<TKey>
{
}
