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

      List<DiskCompactByteList.FixedSizeBlockInfo> compressed = DiskCompactByteList.AllocateBlocks(x);
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
    byte[] data = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
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

  [TestMethod]
  public void T004_MultipleSmallAppends()
  {
    var dmb = new DiskBlockManager();
    File.Delete("CompactByteListTest4.dat");
    dmb.CreateOrOpen("CompactByteListTest4.dat");

    DiskCompactByteList? list = dmb.CompactByteListFactory.AppendNew();
    list.Address.Should().NotBe(0);

    byte[] data = new byte[15000];
    var random = new Random();

    for (int x = 0; x < 15000; x++)
    {
      byte newValue = (byte) random.Next(0, 255);
      data[x] = newValue;
      list.AppendData(new[] { newValue }, 1);
    }

    int index = 0;
    var cursor = new DiskCompactByteListCursor(list);
    while (cursor.MoveNext())
    {
      byte currentValue = (byte) cursor.Current;
      currentValue.Should().Be(data[index], $"Wrong value at index={index}");
      index++;
    }

    index.Should().Be(15000);
  }
}
