using Eugene;
using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests;

using Enumerators;
using Linq;

[TestClass]
public class FastIntersectTests
{
  [TestMethod]
  public void T001_SmallIntersectTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("fastintersect1.dat");

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew(5);
    DiskBTree<int, int> btree2 = btreeFactory.AppendNew(5);

    int[] keys1 = { 1, 7, 9, 11, 13, 17, 23, 29, 31, 33, 39, 47, 50, 55, 61, 67, 69, 70, 73, 79 };
    int[] keys2 = { 1, 3, 9, 67, 71, 75, 79, 89, 91, 93, 95, 97, 99, 101, 103, 105, 107, 109, 111, 113 };

    for (int x = 0; x < keys1.Length; x++)
    {
      btree1.Insert(keys1[x], 100 + x);
    }

    for (int x = 0; x < keys2.Length; x++)
    {
      btree2.Insert(keys2[x], 200 + x);
    }

    IFastIntersectEnumerator<int, int> intersectEnumerator = btree1.FastIntersect(btree2).GetFastEnumerator();

    intersectEnumerator.MoveNext();
    intersectEnumerator.CurrentKey.Should().Be(1, "Should find the first key in the list");
    intersectEnumerator.CurrentData1.Should().Be(100, "Should match data element of first list");
    intersectEnumerator.CurrentData2.Should().Be(200, "Should match data element of second list");

    intersectEnumerator.MoveUntilGreaterThanOrEqual(67);
    intersectEnumerator.CurrentKey.Should().Be(67, "Should intersection key in the list");
    intersectEnumerator.CurrentData1.Should().Be(115, "Should match data element of first list");
    intersectEnumerator.CurrentData2.Should().Be(203, "Should match data element of second list");

    intersectEnumerator.MoveNext();
    intersectEnumerator.CurrentKey.Should().Be(79, "Should find the next intersection in the list");
    intersectEnumerator.CurrentData1.Should().Be(119, "Should match data element of first list");
    intersectEnumerator.CurrentData2.Should().Be(206, "Should match data element of second list");

    dmb.Close();
    File.Exists("fastintersect1.dat").Should().BeTrue("File fastintersect1.dat should exist after creating it");

    // Clean Up
    File.Delete("fastintersect1.dat");
  }
}
