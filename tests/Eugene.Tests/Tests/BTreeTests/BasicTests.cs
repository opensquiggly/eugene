namespace Fixie.Internal;

using Eugene;
using Eugene.Collections;
using FluentAssertions;
using TimeWarp.Fixie;

[TestTag(TestTags.Unit)]
public class BasicBTreeTests
{
  public static void T001_BasicTests()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew();
    btree1.Insert(1, 123);
    btree1.Insert(2, 456);
    btree1.Insert(3, 789);

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }
}
