using Eugene.Collections;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eugene.Tests.MSTest.SortedVarIntListTests;

[TestClass]
public class BasicTests
{
  [TestMethod]
  public void T001_AppendDataOneBlock()
  {
    var dmb = new DiskBlockManager();
    File.Delete("VarIntListTest1.dat");
    dmb.CreateOrOpen("VarIntListTest1.dat");
    
    // ULEB128 Encoding Storage Requirements
    // -------------------------------------
    // 0 - 127                 : 1 bytes
    // 128 - 16383             : 2 bytes
    // 16384 - 2097151         : 3 bytes
    // 2097152 - 268435455     : 4 bytes
    // 268435455 - 34359738367 : 5 bytes
    
    ulong[] data = new ulong[] { 1, 1000, 10000, 100000, 1000000, 1000000000, 1000000128 };
    //                           -  ----  -----  ------  -------  ----------  ----------
    //    Diff from Prior Value: 1   999   9000   90000   900000   999000000         128
    //    Bytes Needed to Store: 1     2      2       3        3           5           2
    //    Total Bytes to Store : 1 + 2 + 2 + 3 + 3 + 5 + 2 = 18

    DiskSortedVarIntList? list = dmb.SortedVarIntListFactory.AppendNew();
    list.Address.Should().NotBe(0);

    list.AppendData(data);
    
    var cursor = new DiskSortedVarIntListCursor(list);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1000);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(10000);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(100000);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1000000);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1000000000);
    
    cursor.MoveNext();
    cursor.Current.Should().Be(1000000128);

    cursor.MoveNext().Should().Be(false);
    
    dmb.Close();
    File.Delete("VarIntListTest1.dat");
  }
}
