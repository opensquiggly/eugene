using Eugene;
using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests;

using Enumerators;
using Linq;

[TestClass]
public class FastUnionTests
{
  [TestMethod]
  public void T001_SmallUnionTest()
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

    IFastUnionEnumerator<int, int> unionEnumerator = btree1.FastUnion(btree2).GetFastEnumerator();

    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(1, "Should find the first key in the list");
    unionEnumerator.CurrentData.Should().Be(100, "Should match data element of first list");
    
    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(1, "Should find the first key in the list");
    unionEnumerator.CurrentData.Should().Be(200, "Should match data element of second list");
    
    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(3, "Should find keys2[1] ");
    unionEnumerator.CurrentData.Should().Be(201, "Should match keys2[1] data");

    unionEnumerator.MoveUntilGreaterThanOrEqual(71);
    unionEnumerator.CurrentKey.Should().Be(71, "Should find keys1[4] ");
    unionEnumerator.CurrentData.Should().Be(204, "Should match keys2[4] data");

    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(73, "Should find keys2[18] ");
    unionEnumerator.CurrentData.Should().Be(118, "Should match keys2[18] data");

    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(75, "Should find keys2[5] ");
    unionEnumerator.CurrentData.Should().Be(205, "Should match keys2[5] data");
    
    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(79, "Should find keys1[19] ");
    unionEnumerator.CurrentData.Should().Be(119, "Should match keys1[19] data");
    
    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(79, "Should find keys2[6] ");
    unionEnumerator.CurrentData.Should().Be(206, "Should match keys2[6] data");
    
    unionEnumerator.MoveNext();
    unionEnumerator.CurrentKey.Should().Be(89, "Should find keys2[7] ");
    unionEnumerator.CurrentData.Should().Be(207, "Should match keys2[7] data");

    dmb.Close();
    File.Exists("fastintersect1.dat").Should().BeTrue("File fastintersect1.dat should exist after creating it");

    // Clean Up
    File.Delete("fastintersect1.dat");
  }
}
