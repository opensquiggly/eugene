## About DiskArray

The DiskArray data structure is used to store a contiguous section of data blocks
within the disk file. A DiskArray has a maximum size, which cannot be exceeded after
it is created. The number of elements in the array can range from 0 up to and including
the maximum size.

## Creating a DiskArray Factory

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

## Creating a DiskArray Instance

Sample Code:
```
// Once we have a factory, we can create new arrays from that factory
var myArray = factory.AppendNew(100); // Maximum items = 100
```

## Adding Items to a DiskArray

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

## Saving the DiskArray Address for Later Retrieval

Once we've created the DiskArray, we'll need to save off its data address somewhere so we'll
be able to load the array again later.

You can access the arrays address using the ```Address``` property and save it either in one
of your other data structures, or for your top-most data structures, you can store the address
in your data file header block. DiskArrayManager provides 8 pre-defined address fields that can
be used for this purpose.

Sample Code:
```
HeaderBlock headerBlock = diskBlockManager.GetHeaderBlock();
headerBlock.Address1 = myArray.Address;
diskBlockManager.WriteHeaderBlock(ref headerBlock);
```

Note: If you have more than 8 root data structures that you need to store, then create a new
block type to store the addresses for all of them, create a single data block for that
block type, and store the address of that data block in your header block.

## Reloading an Existing DiskArray

Use the ```LoadExisting``` method in the DiskArrayFactory to reload an existing array.

Sample Code:
```
HeaderBlock headerBlock = diskBlockManager.GetHeaderBlock();
var myNewArray = factory.LoadExisting(headerBlock.Address1);
```

## Getting the Count

Sample Code:
```
Console.WriteLine($"Count = {myNewArray.Count}");
```

## Accessing Array Elements With the Indexer

Sample Code
```
Console.WriteLine($"Element At Index 0 ... Data1 = {myNewArray[0].Data1}");
Console.WriteLine($"Element At Index 0 ... Data2 = {myNewArray[0].Data2}");
Console.WriteLine($"Element At Index 1 ... Data1 = {myNewArray[1].Data1}");
Console.WriteLine($"Element At Index 1 ... Data2 = {myNewArray[1].Data2}");
```