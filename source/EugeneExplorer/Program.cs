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

    bool finished = false;

    while (!finished)
    {
      ClearScreen();
      Console.WriteLine("\nWelcome to Eugene Explorer");
      Console.WriteLine();
      PrintCurrentDataFileStatus();
      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("---------");
      Console.WriteLine("1. Create and open new data file");
      Console.WriteLine("2. Open an existing data file");
      Console.WriteLine("3. Close current data file");
      Console.WriteLine("4. Print registered block type sizes");
      Console.WriteLine("5. Explore or modify a data structure");
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
          CloseCurrentDataFile();
          break;

        case "4":
          PrintRegisteredBlockTypes();
          break;

        case "5":
          ExploreDataStructures();
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

  private static void ClearScreen()
  {
    Console.Write("\u001b[2J\u001b[H");
  }

  private static void Pause()
  {
    Console.WriteLine();
    Console.Write("Press <Enter> to return to Main Menu ... ");
    Console.ReadLine();
  }

  private static void CreateNewDataFile()
  {
    Console.WriteLine();
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
    Console.WriteLine();
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
      Console.WriteLine();
      Console.WriteLine($"The data file '{FileName}' is currently open.");
      Console.Write("Are you sure want to close it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() != "y")
      {
        return;
      }
      Console.WriteLine($"Closing current data file: {FileName}");
    }

    IsOpen = false;
  }

  private static void PrintRegisteredBlockTypes()
  {
    int index = 0;

    ClearScreen();
    Console.WriteLine();
    Console.WriteLine("Registered Block Types");
    Console.WriteLine("----------------------");

    foreach (BlockTypeMetadataBlock btmb in DiskBlockManager.BlockTypeMetadataBlocksList)
    {
      Console.WriteLine($"Index: {index} Item Size: {btmb.ItemSize}");
      index++;
    }

    Pause();
  }

  private static void AddNewDataStructure()
  {
    ClearScreen();
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
        DiskLinkedListFactory<long> listFactory = DiskBlockManager.LinkedListManager.CreateFactory<long>(DiskBlockManager.LongBlockType);
        infoBlock.DataAddress = listFactory.AppendNew().Address;
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

    ClearScreen();

    Console.WriteLine();
    Console.WriteLine("Here are your current data structures:");
    Console.WriteLine("--------------------------------------");

    int index = 0;

    while (!position.IsPastTail)
    {
      DiskImmutableString dsName = DiskBlockManager.ImmutableStringFactory.LoadExisting(position.Value.NameAddress);

      Console.WriteLine($"{index + 1}: Type: {dsiBlock.Type} Name: {dsName.GetValue()}");
      position.Next();
      index++;
    }

    Pause();
  }

  private static void ExploreDataStructures()
  {
    DiskLinkedListFactory<DataStructureInfoBlock> factory =
      DiskBlockManager.LinkedListManager.CreateFactory<DataStructureInfoBlock>(DataStructureBlockTypeIndex);

    DiskLinkedList<DataStructureInfoBlock> list = factory.LoadExisting(DiskBlockManager.GetHeaderBlock().Address1);

    while (true)
    {
      ClearScreen();
      Console.WriteLine();
      Console.WriteLine("Enter a data structure to explore:");
      Console.WriteLine("----------------------------------");

      int index = 0;
      DiskLinkedList<DataStructureInfoBlock>.Position position = list.GetFirst();
      DataStructureInfoBlock dsiBlock = position.Value;

      while (!position.IsPastTail)
      {
        DiskImmutableString dsName = DiskBlockManager.ImmutableStringFactory.LoadExisting(position.Value.NameAddress);

        Console.WriteLine(
          $"{index + 1}: Type: {dsiBlock.Type} Name: {dsName.GetValue()} Data Address: {position.Value.DataAddress}");
        position.Next();
        index++;
      }

      Console.WriteLine();
      Console.WriteLine("Other Options");
      Console.WriteLine("-------------");
      Console.WriteLine("A. Add new data structure");
      Console.WriteLine("X. Exit to Main Menu");

      Console.WriteLine();
      Console.Write("Enter Selection: ");
      string response = Console.ReadLine();

      if (response.ToLower() == "a")
      {
        AddNewDataStructure();
      }
      else if (response.ToLower() == "x")
      {
        break;
      }
      else if (TryParse(response, out int responseVal))
      {
        DiskLinkedListFactory<long> lllFactory =
          DiskBlockManager.LinkedListManager.CreateFactory<long>(DiskBlockManager.LongBlockType);
        Console.WriteLine($"Loading linked list at address: {list[responseVal - 1].DataAddress}");
        DiskLinkedList<long> linkedList = lllFactory.LoadExisting(list[responseVal - 1].DataAddress);
        var explorer = new LinkedListDataExplorer<long>(linkedList);
        explorer.Explore();
      }
    }
  }
}
