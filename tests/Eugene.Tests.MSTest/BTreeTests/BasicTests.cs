using Eugene;
using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests;

using System.Runtime.InteropServices;

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
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew(100);
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
  public void T003_LargeSortedInsertTest()
  {
    string filename = "btreetest.dat";

    var dmb = new DiskBlockManager();
    if (File.Exists(filename))
    {
      File.Delete(filename);
    }

    dmb.CreateOrOpen(filename);

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew(100);

    for (int x = 1; x <= 100000; x++)
    {
      btree1.Insert(x, x + 100000).Should().BeTrue($"Insert node should succeed x = {x}");
    }

    dmb.Close();
    dmb.CreateOrOpen(filename);

    for (int x = 1; x < 100000; x++)
    {
      btree1.Find(x).Should().Be(x + 100000, $"Value should match what we put into the tree: x = {x}");
    }

    dmb.Close();
    File.Exists(filename).Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete(filename);
  }

  [TestMethod]
  public void T004_TinyInsertTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");
    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew(5);

    btree1.Insert(1, 101);
    btree1.Insert(2, 102);
    btree1.Insert(3, 103);
    btree1.Insert(4, 104);
    btree1.Insert(5, 105);
    btree1.Insert(6, 106);
    btree1.Insert(7, 107);
    btree1.Insert(8, 108);
    btree1.Insert(9, 109);
    btree1.Insert(10, 110);
    btree1.Print();

    // btree1.Insert(5, 105);
    //
    // Console.WriteLine("After inserting 5 nodes");
    // btree1.Print();
    //

    btree1.Find(1).Should().Be(101, "Value should match");
    btree1.Find(2).Should().Be(102, "Value should match");
    btree1.Find(3).Should().Be(103, "Value should match");
    btree1.Find(4).Should().Be(104, "Value should match");
    btree1.Find(5).Should().Be(105, "Value should match");
    btree1.Find(6).Should().Be(106, "Value should match");

    //
    // Console.WriteLine();
    // Console.WriteLine("After inserting 5th node");
    // btree1.Print();

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }

  [TestMethod]
  public void T005_SmallRandomInsertTest()
  {
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("btreetest.dat");

    DiskBTreeFactory<int, int> btreeFactory = dmb.BTreeManager.CreateFactory<int, int>(dmb.IntBlockType, dmb.IntBlockType);
    DiskBTree<int, int> btree1 = btreeFactory.AppendNew(5);

    btree1.Insert(1, 123);
    btree1.Insert(10, 456);
    btree1.Insert(5, 789);
    btree1.Insert(3, 1234);
    btree1.Insert(2, 5678);
    btree1.Insert(9, 3456);
    btree1.Insert(7, 4567);
    btree1.Insert(8, 9876);
    btree1.Insert(6, 2222);
    btree1.Insert(4, 3333);

    btree1.Print();

    dmb.Close();
    dmb.CreateOrOpen("btreetest.dat");

    btree1.Find(4).Should().Be(3333, "Value should match what was inserted");
    btree1.Find(6).Should().Be(2222, "Value should match what was inserted");
    btree1.Find(8).Should().Be(9876, "Value should match what was inserted");
    btree1.Find(7).Should().Be(4567, "Value should match what was inserted");
    btree1.Find(9).Should().Be(3456, "Value should match what was inserted");
    btree1.Find(2).Should().Be(5678, "Value should match what was inserted");
    btree1.Find(3).Should().Be(1234, "Value should match what was inserted");
    btree1.Find(5).Should().Be(789, "Value should match what was inserted");
    btree1.Find(10).Should().Be(456, "Value should match what was inserted");
    btree1.Find(1).Should().Be(123, "Value should match what was inserted");

    dmb.Close();
    File.Exists("btreetest.dat").Should().BeTrue("File testfile.dat should exist after creating it");

    // Clean Up
    File.Delete("testfile.dat");
  }
}
