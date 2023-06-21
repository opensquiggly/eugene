using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Eugene;
using Eugene.Collections;

namespace Eugene.Tests;

[TestClass]
public class DiskBlockBasicTests
{
  [TestMethod]
  public void T001_CreateTest()
  {
    var dmb = new DiskBlockManager();
    dmb.RegisterBlockType<DataBlock>();
    dmb.CreateOrOpen("testfile.dat");
    dmb.Close();
    File.Exists("testfile.dat").Should().BeTrue("File testfile.dat should exist after creating it");
    
    // Clean Up
    File.Delete("testfile.dat");
  }

  [TestMethod]
  public void T002_BasicReadWriteTest()
  {
    if (File.Exists("readwrite.dat"))
    {
      File.Delete("readwrite.dat");
    }
    
    var dmb = new DiskBlockManager();
    int dataBlockType = dmb.RegisterBlockType<DataBlock>();
    dmb.CreateOrOpen("readwrite.dat");
    var addresses = new List<long>();
    
    // Future enhancement for stronger type checking
    // var dataBlockManager = dmb.CreateBlockManager<DataBlock>(dataBlockType);
    // dataBlockManager.AppendBlock(ref dataBlock);

    // Write some data, save the data block addresses
    for (int x = 0; x < 100; x++)
    {
      DataBlock dataBlock = default;
      dataBlock.Value = x;
      addresses.Add(dmb.AppendDataBlock(dataBlockType, ref dataBlock));
    }
    
    dmb.Close();
    File.Exists("readwrite.dat").Should().BeTrue("File readwrite.dat should exist after creating it");
    
    // Reopen the file
    dmb.CreateOrOpen("readwrite.dat");

    // Now read the file back and see if the data is correct
    for (int x = 0; x < 100; x++)
    {
      dmb.ReadDataBlock(dataBlockType, addresses[x], out DataBlock dataBlock);
      dataBlock.Value.Should().Be(x, "Value should match was was written to disk");
    }

    dmb.Close();
    dmb.CreateOrOpen("readwrite.dat");

    // Modify each data block by adding 100 to the original value
    for (int x = 0; x < 100; x++)
    {
      dmb.ReadDataBlock(dataBlockType, addresses[x], out DataBlock dataBlock);
      dataBlock.Value = 100 + x;
      dmb.WriteDataBlock(dataBlockType, addresses[x], ref dataBlock);
    }

    dmb.Close();
    dmb.CreateOrOpen("readwrite.dat");
    
    // Now read the file back and see if the modified data is correct
    for (int x = 0; x < 100; x++)
    {
      dmb.ReadDataBlock(dataBlockType, addresses[x], out DataBlock dataBlock);
      dataBlock.Value.Should().Be(x + 100, "Value should match was was written to disk");
    }

    // Clean Up
    File.Delete("readwrite.dat");
  }

  [TestMethod]
  public void T003_LinkedListTests()
  {
    if (File.Exists("linkedlist.dat"))
    {
      File.Delete("linkedlist.dat");
    }
    
    var dmb = new DiskBlockManager();
    short longBlockType = dmb.RegisterBlockType<long>();
    var linkedListFactory = dmb.LinkedListManager.CreateFactory<long>(longBlockType);

    dmb.CreateOrOpen("linkedlist.dat");
    var linkedList1 = linkedListFactory.AppendNew();
    dmb.Close();
    
    dmb.CreateOrOpen("linkedlist.dat");
    var linkedList2 = linkedListFactory.LoadExisting(linkedList1.Address);
    for (int x = 1000; x <= 2000; x++)
    {
      linkedList2.AddLast(x);
    }
    dmb.Close();
  }

  [TestMethod]
  public void T004_ArrayTests()
  {
    if (File.Exists("array.dat"))
    {
      File.Delete("array.dat");
    }
    
    var dmb = new DiskBlockManager();
    var arrayFactory = dmb.ArrayOfLongFactory;

    dmb.CreateOrOpen("array.dat");
    
    var array1 = arrayFactory.AppendNew(10);
    array1.AddItem(123);
    array1.AddItem(456);
    array1.AddItem(789);
    array1.AddItem(2);
    array1.AddItem(3);
    array1.AddItem(5);

    array1[2].Should().Be(789, "Value should match what is written to disk");
    dmb.Close();
    
    dmb.CreateOrOpen("array.dat");
    var array2 = arrayFactory.LoadExisting(array1.Address);
    
    array2[0].Should().Be(123, "Value should match what is written to disk");
    array2[1].Should().Be(456, "Value should match what is written to disk");
    array2[2].Should().Be(789, "Value should match what is written to disk");
    array2[3].Should().Be(2, "Value should match what is written to disk");
    array2[4].Should().Be(3, "Value should match what is written to disk");
    array2[5].Should().Be(5, "Value should match what is written to disk");
    
    dmb.Close();
  }

  [TestMethod]
  public void T005_FixedStringTests()
  {
    if (File.Exists("strings.dat"))
    {
      File.Delete("strings.dat");
    }
    
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("strings.dat");

    var string1 = dmb.FixedStringFactory.AppendEmpty(20);
    string1.GetValue().Should().Be("", "Initial value should be an empty string");
    string1.SetValue("Kevin Dietz");
    string1.GetValue().Should().Be("Kevin Dietz", "String should match what is written to disk");
    string1.SetValue("Kevin Dietz and Christian Dietz");
    string1.GetValue().Should().Be("Kevin Dietz and Chri", "String should be truncated to max length");

    dmb.Close();
    
    dmb.CreateOrOpen("strings.dat");
    var string2 = dmb.FixedStringFactory.LoadExisting(string1.Address);
    string2.GetValue().Should().Be("Kevin Dietz and Chri", "Previous value written to disk should be retained");

    dmb.Close();
  }
  
  [TestMethod]
  public void T006_ImmutableStringTests()
  {
    if (File.Exists("strings.dat"))
    {
      File.Delete("strings.dat");
    }
    
    var dmb = new DiskBlockManager();
    dmb.CreateOrOpen("strings.dat");

    var string1 = dmb.ImmutableStringFactory.Append("The quick brown fox jumps over the lazy dog");
    var string2 = dmb.ImmutableStringFactory.Append("This is a test of the emergency broadcast system");
    string1.GetValue().Should().Be("The quick brown fox jumps over the lazy dog", "Initial value should be available");
    string2.GetValue().Should().Be("This is a test of the emergency broadcast system", "Initial value should be available");

    dmb.Close();
    
    dmb.CreateOrOpen("strings.dat");
    var newString1 = dmb.ImmutableStringFactory.LoadExisting(string1.Address);
    var newString2 = dmb.ImmutableStringFactory.LoadExisting(string2.Address);
    newString1.GetValue().Should().Be("The quick brown fox jumps over the lazy dog", "Previous value written to disk should be retained");
    newString2.GetValue().Should().Be("This is a test of the emergency broadcast system", "Previous value written to disk should be retained");

    dmb.Close();
  }  

  private struct DataBlock
  {
    public long Value { get; set; }
  }
}
