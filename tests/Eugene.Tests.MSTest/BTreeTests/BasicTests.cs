using Eugene;
using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests;

[TestClass]
public class BTreeTests
{
  [TestMethod]
  public void T001_CreateTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");
    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("btreetest.dat");
  }

  [TestMethod]
  public void T002_CreateAndInsertTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew();
    btree1.Insert(1, 123);
    btree1.Insert(2, 456);
    btree1.Insert(3, 789);

    btree1.Find(1).Should().Be(123, "Value should match what we put into the tree");
    btree1.Find(2).Should().Be(456, "Value should match what we put into the tree");
    btree1.Find(3).Should().Be(789, "Value should match what we put into the tree");

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }

  [TestMethod]
  public void T003_MultiInsertTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew();

    for (int x = 1; x <= 100000; x++)
    {
      btree1.Insert(x, x + 100000).Should().BeTrue($"Insert node should succeed x = {x}");
    }

    dmb.Close();
    dmb.CreateOrOpen("btreetest.dat");

    for (int x = 1; x < 100000; x++)
    {
      btree1.Find(x).Should().Be(x + 100000, $"Value should match what we put into the tree: x = {x}");
    }

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }
}
