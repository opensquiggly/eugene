using Eugene;

Console.WriteLine("Welcome to Eugene Explorer");
Console.WriteLine("--------------------------");

while (true)
{
  Console.WriteLine("1. Create a new file");
  Console.WriteLine("2. Open an existing file");
  Console.WriteLine("3. Show existing data structures");
  Console.WriteLine("4. Add a new data structure");
  Console.WriteLine("5. Manage an existing data structure");
  Console.WriteLine("X. Exit program");

  string response = Console.ReadLine();

  if (response.ToLower() == "x")
  {
    break;
  }
}

