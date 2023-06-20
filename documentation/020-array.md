# About DiskArray

The DiskArray data structure is used to store a contiguous section of data blocks
within the disk file. A DiskArray has a maximum size, which cannot be exceeded after
it is created. The number of elements in the array can range from 0 up to and including
the maximum size.

# Creating a DiskArray Factory

Sample Code:
```
using Eugene;
using Eugene.Collections;

public struct MyDataBlock
{
  // Define some data fields
  public int Data1 { get; set; }
  public int Data2 { get; set; }
}

var diskBlockManager = new DiskBlockManager();
short myDataBlockType = diskBlockManager.RegisterBlockType<MyDataBlock>();
diskBlockManager.CreateOrOpen("hello.dat");

// Create a factory that lets us create new arrays of type MyDataBlock
var factory = diskBlockManager.ArrayManager.CreateFactory<MyDataBlock>(myDataBlockType);
```

# Creating a DiskArray Instance

Sample Code:
```
// Once we have a factory, we can create new arrays from that factory
var myArray = factory.AppendNew(100); // Maximum items = 100
```

# Adding Items to a DiskArray

Sample Code:
```
// Now we can add items to the array
MyDataBlock myData = default;
myData.Data1 = 1234;
myData.Data2 = 5678;
myArray.AddItem(myData);

myData.Data1 = 2345;
myData.Data2 = 6789;
myArray.AddItem(myData);
```