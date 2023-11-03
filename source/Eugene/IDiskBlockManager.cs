namespace Eugene;

public interface IDiskBlockManager
{
  public short RegisterBlockType<TStruct>() where TStruct : struct;
  void CreateOrOpen(string filePath);
  void Flush();
  public void ReadBlockMetadataBlock(long address, out BlockMetadataBlock input);
  public void ReadDataBlock<TStruct>(int blockTypeIndex, long address, out TStruct input) where TStruct : struct;
  public void WriteDataBlock<TStruct>(int blockTypeIndex, long address, ref TStruct input) where TStruct : struct;
  public long AppendDataBlock<TStruct>(int blockTypeIndex, ref TStruct input) where TStruct : struct;
  public long AppendDataBlockArray<TData>(int blockTypeIndex, int count) where TData : struct;

  public void ReadDataBlockArrayEntry<TStruct>(int blockTypeIndex, long baseAddress, int index, out TStruct input)
    where TStruct : struct;

  public void WriteDataBlockArrayEntry<TStruct>(int blockTypeIndex, long baseAddress, int index, ref TStruct input)
    where TStruct : struct;

  public void DeleteDataBlock(int blockTypeIndex, long address);
}
