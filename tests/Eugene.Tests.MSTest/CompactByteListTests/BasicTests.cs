using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Eugene.Tests.MSTest.CompactByteListTests;

[TestClass]
public class BasicTests
{
  [TestMethod]
  public void T001_BlockAllocationTest()
  {
    for (int x = 1; x <= 200; x++)
    {
      var solution = DiskCompactByteList.AllocateRawBlocks(x);
      bool first = true;
      // Console.Write($"N = {x} ... ");
      Console.Write("N = {0:d5} ... ", x);
      // foreach (var result in solution)
      // {
      //   if (!first)
      //   {
      //     Console.Write(", ");
      //   }
      //   Console.Write($"{result.BlockSize} / {result.BytesStored}");
      //   first = false;
      // }
      // Console.WriteLine();
      // Console.Write(" Compress  => ");
      
      var compressed = DiskCompactByteList.AllocateBlocks(x);
      first = true;
      foreach (var result in compressed)
      {
        if (!first)
        {
          Console.Write(", ");
        }
        Console.Write($"{result.BlockSize} / {result.BytesStored}");
        first = false;
      }

      Console.WriteLine();
    }
  }

  [TestMethod]
  public void T002_CreateTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("ByteListTest.dat");
    byte[] data = new byte[] { 1, 2, 3 };
    var compactByteList = dmb.CompactListFactory.AppendNew(data);
    compactByteList.Address.Should().NotBe(0, "We should have an address where the list starts");

    compactByteList.AppendData(data);

    dmb.Close();
    File.Delete("ByteListTest.dat");
  }
}
