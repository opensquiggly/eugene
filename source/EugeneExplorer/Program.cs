using Eugene;

namespace EugeneExplorer;

using System.Globalization;

internal static class Program
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  static Program()
  {
    DiskBlockManager = new DiskBlockManager();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Application Entry Point
  // /////////////////////////////////////////////////////////////////////////////////////////////

  internal static void Main(string[] args)
  {
    Console.WriteLine("Welcome to Eugene Explorer\n\n");

    bool finished = false;

    while (!finished)
    {
      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("---------");
      Console.WriteLine("1. Create a new data file");
      Console.WriteLine("2. Open an existing data file");
      Console.WriteLine("3. Show existing data structures");
      Console.WriteLine("4. Add a new data structure");
      Console.WriteLine("5. Manage an existing data structure");
      Console.WriteLine("X. Exit program");
      Console.WriteLine();
      Console.Write("Enter selection: ");

      string response = Console.ReadLine();

      switch (response.ToLower())
      {
        case "1": CreateNewDataFile(); break;
        case "x": finished = true; break;
      }
    }

    DiskBlockManager.Close();
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////
  
  private static DiskBlockManager DiskBlockManager { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static void CreateNewDataFile()
  {
    Console.Write("Enter data file name: ");
    string filename = Console.ReadLine();
    DiskBlockManager.CreateOrOpen(filename);
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
  }
}
