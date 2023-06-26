using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests;

[TestClass]
public class SortedArrayBasicTests
{
  [TestMethod]
  public void T001_AddSingleItemTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("sortedarray.dat");

    DiskSortedArray<int> array1 = dmb.SortedArrayOfIntFactory.AppendNew(10);
    array1.AddItem(1);
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");

    dmb.Close();
    File.Delete("btreetest.dat");
  }

  [TestMethod]
  public void T002_SmallAddTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("sortedarray.dat");

    DiskSortedArray<int> array1 = dmb.SortedArrayOfIntFactory.AppendNew(10);

    array1.Count.Should().Be(0, "Verify the array size is correct");

    array1.AddItem(3);
    array1.Count.Should().Be(1, "Verify the array size is correct");
    array1.FindLastEqual(3).Should().Be(0, "Element should be in the array at the expected index");

    array1.AddItem(7);
    array1.Count.Should().Be(2, "Verify the array size is correct");
    array1.FindLastEqual(3).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(1, "Element should be in the array at the expected index");

    array1.AddItem(1);
    array1.Count.Should().Be(3, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(2, "Element should be in the array at the expected index");

    array1.AddItem(2);
    array1.Count.Should().Be(4, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(3, "Element should be in the array at the expected index");

    array1.AddItem(5);
    array1.Count.Should().Be(5, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(4, "Element should be in the array at the expected index");

    array1.AddItem(10);
    array1.Count.Should().Be(6, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(5, "Element should be in the array at the expected index");

    array1.AddItem(8);
    array1.Count.Should().Be(7, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(8).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(6, "Element should be in the array at the expected index");

    array1.AddItem(9);
    array1.Count.Should().Be(8, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(8).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindLastEqual(9).Should().Be(6, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(7, "Element should be in the array at the expected index");

    array1.AddItem(4);
    array1.Count.Should().Be(9, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(4).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindLastEqual(8).Should().Be(6, "Element should be in the array at the expected index");
    array1.FindLastEqual(9).Should().Be(7, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(8, "Element should be in the array at the expected index");

    array1.AddItem(6);
    array1.Count.Should().Be(10, "Verify the array size is correct");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(4).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(6).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(6, "Element should be in the array at the expected index");
    array1.FindLastEqual(8).Should().Be(7, "Element should be in the array at the expected index");
    array1.FindLastEqual(9).Should().Be(8, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(9, "Element should be in the array at the expected index");

    dmb.Close();
    File.Delete("btreetest.dat");
  }

  [TestMethod]
  public void T003_FindOperatorsTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("sortedarray.dat");

    DiskSortedArray<int> array1 = dmb.SortedArrayOfIntFactory.AppendNew(11);

    array1.Count.Should().Be(0, "Verify the array size is correct");

    array1.AddItem(3);
    array1.AddItem(7);
    array1.AddItem(1);
    array1.AddItem(2);
    array1.AddItem(5);
    array1.AddItem(10);
    array1.AddItem(8);
    array1.AddItem(9);
    array1.AddItem(4);
    array1.AddItem(6);
    array1.AddItem(5);

    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(2).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindLastEqual(3).Should().Be(2, "Element should be in the array at the expected index");
    array1.FindLastEqual(4).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindFirstEqual(5).Should().Be(4, "Element should be in the array at the expected index");
    array1.FindLastEqual(5).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindLastEqual(6).Should().Be(6, "Element should be in the array at the expected index");
    array1.FindLastEqual(7).Should().Be(7, "Element should be in the array at the expected index");
    array1.FindLastEqual(8).Should().Be(8, "Element should be in the array at the expected index");
    array1.FindLastEqual(9).Should().Be(9, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(10, "Element should be in the array at the expected index");

    array1.FindLastLessThan(1).Should().Be(-1, "Element should be in the array at the expected index");
    array1.FindLastLessThanOrEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindFirstGreaterThan(1).Should().Be(1, "Element should be in the array at the expected index");
    array1.FindFirstGreaterThanOrEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindFirstEqual(1).Should().Be(0, "Element should be in the array at the expected index");
    array1.FindLastEqual(1).Should().Be(0, "Element should be in the array at the expected index");

    array1.FindFirstGreaterThan(10).Should().Be(-1, "Element should be in the array at the expected index");
    array1.FindFirstGreaterThanOrEqual(10).Should().Be(10, "Element should be in the array at the expected index");
    array1.FindLastLessThan(10).Should().Be(9, "Element should be in the array at the expected index");
    array1.FindLastLessThanOrEqual(10).Should().Be(10, "Element should be in the array at the expected index");
    array1.FindFirstEqual(10).Should().Be(10, "Element should be in the array at the expected index");
    array1.FindLastEqual(10).Should().Be(10, "Element should be in the array at the expected index");

    array1.FindLastLessThan(5).Should().Be(3, "Element should be in the array at the expected index");
    array1.FindLastLessThanOrEqual(5).Should().Be(5, "Element should be in the array at the expected index");
    array1.FindFirstGreaterThan(5).Should().Be(6, "Element should be in the array at the expected index");
    array1.FindFirstGreaterThanOrEqual(5).Should().Be(4, "Element should be in the array at the expected index");

    dmb.Close();
    File.Delete("btreetest.dat");
  }
}
