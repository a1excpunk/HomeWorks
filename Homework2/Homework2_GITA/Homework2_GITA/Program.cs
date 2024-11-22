// See https://aka.ms/new-console-template for more information
Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.Blue;
Console.Clear();

Console.WriteLine("Aleksandra Sauri");
string input = "";

Console.WriteLine("Say anything: ");
input = Console.ReadLine();

string output = string.IsNullOrEmpty(input) ? "Nothing" : input;

Console.WriteLine($"you said: {output}. Yeah Shocking!");

Console.ReadLine();

