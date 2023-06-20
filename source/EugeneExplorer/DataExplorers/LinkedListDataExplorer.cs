namespace EugeneExplorer.DataExplorers;

public class LinkedListDataExplorer<TData> where TData : struct
{
  public LinkedListDataExplorer(DiskLinkedList<long> linkedList)
  {
    LinkedList = linkedList;
  }

  private DiskLinkedList<long> LinkedList { get; }

  private void ClearScreen()
  {
    Console.Write("\u001b[2J\u001b[H");
  }

  private void Pause()
  {
    Console.WriteLine();
    Console.Write("Press <Enter> to return to Main Menu ... ");
    Console.ReadLine();
  }

  private void PrintListValues()
  {
    int index = 0;
    DiskLinkedList<long>.Position position = LinkedList.GetFirst();

    ClearScreen();
    Console.WriteLine();
    Console.WriteLine($"Count: {LinkedList.Count}");
    Console.WriteLine();

    while (!position.IsPastTail)
    {
      Console.WriteLine($"{index}: {position.Value}");
      index++;
      position.Next();
    }

    Pause();
  }

  private void AddNewValue()
  {
    Console.Write("Value to add: ");
    string response = Console.ReadLine();
    if (long.TryParse(response, out long val))
    {
      LinkedList.AddLast(val);
    }
  }

  public void Explore()
  {
    bool finished = false;

    while (!finished)
    {
      ClearScreen();
      Console.WriteLine();
      Console.WriteLine("Linked List Data Explorer Menu");
      Console.WriteLine("------------------------------");
      Console.WriteLine("1. Print all values in list");
      Console.WriteLine("2. Add a new value to end of list");
      Console.WriteLine("X. Exit to Explorer Menu");
      Console.WriteLine();
      Console.Write("Enter Selection: ");
      string response = Console.ReadLine();

      switch (response.ToLower())
      {
        case "1":
          PrintListValues();
          break;

        case "2":
          AddNewValue();
          break;

        case "x":
          finished = true;
          break;
      }
    }
  }
}
