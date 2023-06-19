namespace EugeneExplorer.DataExplorers;

public class LinkedListDataExplorer<TData> where TData : struct
{
  public LinkedListDataExplorer(DiskLinkedList<long> linkedList)
  {
    LinkedList = linkedList;
  }
  
  private DiskLinkedList<long> LinkedList { get; }

  private void PrintListValues()
  {
    int index = 0;
    var position = LinkedList.GetFirst();
    
    Console.WriteLine($"Count: {LinkedList.Count}");
    
    while (!position.IsPastTail)
    {
      Console.WriteLine($"{index}: {position.Value}");
      index++;
      position.Next();
    }
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
      Console.WriteLine();
      Console.WriteLine("Linked List Data Explorer Menu");
      Console.WriteLine("------------------------------");
      Console.WriteLine("1. Print all values in list");
      Console.WriteLine("2. Add a new value to end of list");
      Console.WriteLine("X. Exit to Main Menu");
      Console.Write("> Enter Selection: ");
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
