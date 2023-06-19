namespace EugeneExplorer;

internal static class Program
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  static Program()
  {
    DiskBlockManager = new DiskBlockManager();
    DataStructureBlockTypeIndex = DiskBlockManager.RegisterBlockType<DataStructureInfoBlock>();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Application Entry Point
  // /////////////////////////////////////////////////////////////////////////////////////////////

  internal static void Main(string[] args)
  {
    Console.WriteLine("\nWelcome to Eugene Explorer");

    bool finished = false;

    while (!finished)
    {
      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("---------");
      PrintCurrentDataFileStatus();
      Console.WriteLine("1. Create and open new data file");
      Console.WriteLine("2. Open an existing data file");
      Console.WriteLine("3. Show current data file status");
      Console.WriteLine("4. Close current data file");
      Console.WriteLine("5. Add a new data structure");
      Console.WriteLine("6. Print registered block type sizes");
      Console.WriteLine("7. Print data structures");
      Console.WriteLine("X. Exit program");
      Console.WriteLine();
      Console.Write("Enter selection: ");

      string response = Console.ReadLine();

      switch (response.ToLower())
      {
        case "1":
          CreateNewDataFile();
          break;

        case "2":
          OpenExistingDataFile();
          break;

        case "3":
          PrintCurrentDataFileStatus();
          break;

        case "4":
          CloseCurrentDataFile();
          break;

        case "5":
          AddNewDataStructure();
          break;

        case "6":
          PrintRegisteredBlockTypes();
          break;
        
        case "7":
          PrintDataStructures();
          break;

        case "x":
          finished = true;
          break;
      }
    }

    DiskBlockManager.Close();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static DiskBlockManager DiskBlockManager { get; }

  private static string FileName { get; set; }

  private static bool IsOpen { get; set; } = false;

  private static short DataStructureBlockTypeIndex { get; set; }

  private static DiskLinkedList<DataStructureInfoBlock> DataStructureInfoList { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static void CreateNewDataFile()
  {
    Console.Write("Enter data file name: ");
    string filename = Console.ReadLine();

    if (File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' already exists.");
      Console.Write("Do you want to delete and recreate it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() == "y")
      {
        File.Delete(filename);
      }
      else
      {
        return;
      }
    }

    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    DiskLinkedListFactory<DataStructureInfoBlock> factory = DiskBlockManager.LinkedListManager.CreateFactory<DataStructureInfoBlock>(DataStructureBlockTypeIndex);
    DataStructureInfoList = factory.AppendNew();

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();
    headerBlock.Address1 = DataStructureInfoList.Address;

    DiskBlockManager.WriteHeaderBlock(ref headerBlock);

    Console.WriteLine($"Data Structure List Stored at Address: {headerBlock.Address1}");

    IsOpen = true;
    FileName = filename;
  }

  private static void OpenExistingDataFile()
  {
    Console.Write("Enter data file name: ");
    string filename = Console.ReadLine();

    if (!File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' does not exist.");
      Console.WriteLine("Use option 1 from the Main Menu to create a new file");
      return;
    }

    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();

    DiskLinkedListFactory<DataStructureInfoBlock> factory = DiskBlockManager.LinkedListManager.CreateFactory<DataStructureInfoBlock>(DataStructureBlockTypeIndex);
    DataStructureInfoList = factory.LoadExisting(headerBlock.Address1);

    Console.WriteLine($"Data Structure List Loaded from Address: {headerBlock.Address1}");
    Console.WriteLine($"There are {DataStructureInfoList.Count} data structures");

    IsOpen = true;
    FileName = filename;

    Console.WriteLine($"File name {filename} is now open for exploration.");
  }

  private static void PrintCurrentDataFileStatus()
  {
    if (IsOpen)
    {
      Console.WriteLine($"The data file '{FileName}' is currently open");
    }
    else
    {
      Console.WriteLine("There is no current data file open");
    }
  }

  private static void CloseCurrentDataFile()
  {
    if (IsOpen)
    {
      Console.WriteLine($"The data file '{FileName}' is currently open.");
      Console.Write("Are you sure want to close it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() != "y")
      {
        return;
      }
      Console.WriteLine($"Closing current data file: {FileName}");
    }
    else
    {
      Console.WriteLine("There is no current data file open");
    }

    IsOpen = false;
  }

  private static void PrintRegisteredBlockTypes()
  {
    int index = 0;

    foreach (BlockTypeMetadataBlock btmb in DiskBlockManager.BlockTypeMetadataBlocksList)
    {
      Console.WriteLine($"Index: {index} Item Size: {btmb.ItemSize}");
      index++;
    }
  }

  private static void AddNewDataStructure()
  {
    Console.WriteLine();
    Console.WriteLine("Enter the type of data structure you wish to create");
    Console.WriteLine("1. Array of Long");
    Console.WriteLine("2. Array of Fixed Strings");
    Console.WriteLine("3. Array of Immutable Strings");
    Console.WriteLine("4. Linked List of Long");
    Console.WriteLine("5. Linked List of Fixed Strings");
    Console.WriteLine("6. Linked List of Immutable Strings");
    Console.Write("> Enter Selection: ");
    string response = Console.ReadLine();

    Console.Write("> Enter Data Structure Name: ");
    string name = Console.ReadLine();

    Console.Write($"> Ready to add new data structure '{name}' (y/n): ");
    string confirm = Console.ReadLine();

    if (confirm.ToLower() != "y")
    {
      return;
    }

    DataStructureInfoBlock infoBlock = default;

    switch (response.ToLower())
    {
      case "4":
        DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(name);
        infoBlock.Type = 4;
        infoBlock.MaxItems = 0;
        infoBlock.NameAddress = nameString.Address;
        DataStructureInfoList.AddLast(infoBlock);
        break;

      default:
        break;
    }
  }

  private static void PrintDataStructures()
  {
    DiskLinkedListFactory<DataStructureInfoBlock> factory = 
      DiskBlockManager.LinkedListManager.CreateFactory<DataStructureInfoBlock>(DataStructureBlockTypeIndex);

    DiskLinkedList<DataStructureInfoBlock> list = factory.LoadExisting(DiskBlockManager.GetHeaderBlock().Address1);
    DiskLinkedList<DataStructureInfoBlock>.Position position = list.GetFirst();
    DataStructureInfoBlock dsiBlock = position.Value;
    
    Console.WriteLine("Here are your current data structures:");
    Console.WriteLine("--------------------------------------");

    while (!position.IsPastTail)
    {
      var dsName = DiskBlockManager.ImmutableStringFactory.LoadExisting(position.Value.NameAddress);

      Console.WriteLine($"Type: {dsiBlock.Type} Name: {dsName.GetValue()}");
      position.Next();
    }

    Console.WriteLine();
    Console.WriteLine("Press <Enter> to return to Main Menu");
    Console.ReadLine();
  }
}
