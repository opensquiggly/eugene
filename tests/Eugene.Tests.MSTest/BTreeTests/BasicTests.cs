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
    File.Delete("testfile.dat");
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

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }
}
