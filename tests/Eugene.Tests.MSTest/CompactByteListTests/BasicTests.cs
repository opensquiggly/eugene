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
      List<DiskCompactByteList.FixedSizeBlockInfo>? solution = DiskCompactByteList.AllocateRawBlocks(x);
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
      foreach (DiskCompactByteList.FixedSizeBlockInfo? result in compressed)
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
  public void T002_AppendAndReadTestOneBlock()
  {
    var dmb = new DiskBlockManager();
    File.Delete("ByteListTest2.dat");
    dmb.CreateOrOpen("ByteListTest2.dat");
    byte[] data = new byte[] { 1, 2, 3, 4, 5, 6, 7  };
    DiskCompactByteList? compactByteList = dmb.CompactByteListFactory.AppendNew(data);
    compactByteList.Address.Should().NotBe(0, "We should have an address where the list starts");

    compactByteList.AppendData(data, data.Length);
    compactByteList.AppendData(data, data.Length);

    var cursor = new DiskCompactByteListCursor(compactByteList);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1);
    cursor.MoveNext();
    cursor.Current.Should().Be(2);
    cursor.MoveNext();
    cursor.Current.Should().Be(3);
    cursor.MoveNext();
    cursor.Current.Should().Be(4);
    cursor.MoveNext();
    cursor.Current.Should().Be(5);
    cursor.MoveNext();
    cursor.Current.Should().Be(6);
    cursor.MoveNext();
    cursor.Current.Should().Be(7);
    cursor.MoveNext();
    cursor.Current.Should().Be(1);
    cursor.MoveNext();
    cursor.Current.Should().Be(2);
    cursor.MoveNext();
    cursor.Current.Should().Be(3);
    cursor.MoveNext();
    cursor.Current.Should().Be(4);
    cursor.MoveNext();
    cursor.Current.Should().Be(5);
    cursor.MoveNext();
    cursor.Current.Should().Be(6);
    cursor.MoveNext();
    cursor.Current.Should().Be(7);

    cursor.MoveNext().Should().Be(false);
    
    dmb.Close();
    File.Delete("ByteListTest2.dat");
  }
  
  [TestMethod]
  public void T003_AppendAndReadTestMultiBlocks()
  {
    var dmb = new DiskBlockManager();
    File.Delete("ByteListTest2.dat");
    dmb.CreateOrOpen("ByteListTest2.dat");
    byte[] data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 };
    DiskCompactByteList? compactByteList = dmb.CompactByteListFactory.AppendNew(data);
    compactByteList.Address.Should().NotBe(0, "We should have an address where the list starts");

    compactByteList.AppendData(data, data.Length);

    var cursor = new DiskCompactByteListCursor(compactByteList);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1);
    cursor.MoveNext();
    cursor.Current.Should().Be(2);
    cursor.MoveNext();
    cursor.Current.Should().Be(3);
    cursor.MoveNext();
    cursor.Current.Should().Be(4);
    cursor.MoveNext();
    cursor.Current.Should().Be(5);
    cursor.MoveNext();
    cursor.Current.Should().Be(6);
    cursor.MoveNext();
    cursor.Current.Should().Be(7);
    cursor.MoveNext();
    cursor.Current.Should().Be(8);
    cursor.MoveNext();
    cursor.Current.Should().Be(9);
    cursor.MoveNext();
    cursor.Current.Should().Be(10);
    cursor.MoveNext();
    cursor.Current.Should().Be(11);
    cursor.MoveNext();
    cursor.Current.Should().Be(12);
    cursor.MoveNext();
    cursor.Current.Should().Be(13);
    cursor.MoveNext();
    cursor.Current.Should().Be(14);
    cursor.MoveNext();
    cursor.Current.Should().Be(15);
    cursor.MoveNext();
    cursor.Current.Should().Be(16);
    cursor.MoveNext();
    cursor.Current.Should().Be(17);    
    
    cursor.MoveNext().Should().Be(false);
    
    dmb.Close();
    File.Delete("ByteListTest2.dat");
  }

  [TestMethod]
  public void T003_AppendTest()
  {
  }
}
