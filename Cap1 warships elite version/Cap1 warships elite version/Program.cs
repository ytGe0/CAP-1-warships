using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.Remoting.Services;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

class Program
{
    static public int Tries = 0; //you get a flare every 10th attempt
    static public int shipno = 5; //no. of ships
    static public int x = 9; //column
    static public int y = 9; //row
    static public int FlareSize = 3; //flare size eg 3x3, 5x5, 7x7 area (must be an odd number for grid)

    public struct ShipType
    {
        public string Name;
        public int Size;
    }

    const string TrainingGame = "Training.txt";

    public List<int> previous = new List<int>();

    private static void GetRowColumn(ref int Row, ref int Column, int Tries)
    {
        Console.WriteLine();
        bool valid = true;
        while (valid)
        {
            if (Tries % 10 == 0 && Tries > 0)
            {
                Console.WriteLine("You can shoot a flare now!");
                try
                {
                    Console.Write("Flare Column: ");
                    Column = Convert.ToInt32(Console.ReadLine());
                    valid = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Try Again");
                }
                if (Column > x) { valid = true; Console.WriteLine("Try Again"); }
            }
            else
            {
                try
                {
                    Console.Write("Column: ");
                    Column = Convert.ToInt32(Console.ReadLine());
                    valid = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Try Again");
                }
                if (Column > x) { valid = true; Console.WriteLine("Try Again"); }
            }
        }
        valid = true;
        while (valid)
        {
            if (Tries % 10 == 0 && Tries > 0)
            {
                try
                {
                    Console.Write("Flare Row: ");
                    Row = Convert.ToInt32(Console.ReadLine());
                    valid = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Try Again");
                }
                if (Row > y) { valid = true; Console.WriteLine("Try Again"); }
            }
            else
            {
                try
                {
                    Console.Write("Row: ");
                    Row = Convert.ToInt32(Console.ReadLine());
                    valid = false;
                }
                catch (FormatException)
                {
                    Console.WriteLine("Try Again");
                }
                if (Row > y) { valid = true; Console.WriteLine("Try Again"); }
            }
        }

    }

    public static void CheckSunk(Char[,] Board, int Row, int Column, ShipType[] ships)
    {
        switch (Board[Column, Row])
        {
            case 'A':
                ships[0].Size--;
                break;
            case 'B':
                ships[1].Size--;
                break;
            case 'S':
                ships[2].Size--;
                break;
            case 'D':
                ships[3].Size--;
                break;
            case 'P':
                ships[4].Size--;
                break;
        }
        for (int i = 0; i < ships.Length; i++)
        {
            if (ships[i].Size == 0)
            {
                Console.Write(ships[i].Name + " was sunk ");
                ships[i].Size = 1;
            }
        }
    }

    private static void MakePlayerMove(ref int Tries, ref char[,] Board, ref ShipType[] Ships)
    {
        int Row = y;
        int Column = x;
        GetRowColumn(ref Row, ref Column, Tries);
        Console.Clear();
        if (Tries % 10 == 0 && Tries > 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("The smoke from the flare has affected visibility!");
            for (int i = -((FlareSize - 1) / 2); i < ((FlareSize - 1) / 2) + 1; i++)
            {
                for (int j = -(FlareSize - 1) / 2; j < ((FlareSize - 1) / 2) + 1; j++)
                {
                    try
                    {
                        if (Board[Column + i, Row + j] != '-' && Board[Column + i, Row + j] != 'm' && Board[Column + i, Row + j] != 'h' && Board[Column + i, Row + j] != '?')
                        {
                            Board[Column + i, Row + j] = '+';
                        }
                        else if (Board[Column + i, Row + j] == 'h')
                        {

                        }
                        else
                        {
                            Board[Column + i, Row + j] = '?';
                        }
                    }
                    catch { }
                }
            }
            Tries++;
        }
        else
        {
            if (Board[Column, Row] == 'm' || Board[Column, Row] == 'h')
            {
                Console.Write("Sorry, you have already shot at the square (" + Column + "," + Row + "). Please try again.");
            }
            else if (Board[Column, Row] == '-')
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Sorry, (" + Column + "," + Row + ") is a miss.");
                Board[Column, Row] = 'm';
                Tries++;
                Console.ResetColor();
                Console.Write(" Attempt: " + Tries);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Hit at (" + Column + "," + Row + ") ");
                CheckSunk(Board, Row, Column, Ships);
                Board[Column, Row] = 'h';
                Tries++;
                Console.ResetColor();
                //loop here
                Console.Write(" Attempt: " + Tries);
            }
        }
        Console.ResetColor();
    }

    private static void SetUpBoard(ref char[,] Board)
    {
        for (int Row = 0; Row < y + 1; Row++)
        {
            for (int Column = 0; Column < x + 1; Column++)
            {
                Board[Column, Row] = '-';
            }
        }
    }

    private static void LoadGame(string TrainingGame, ref char[,] Board)
    {
        string Line = "";
        using (StreamReader BoardFile = new StreamReader(TrainingGame))
        {
            for (int Row = 0; Row < y + 1; Row++)
            {
                Line = BoardFile.ReadLine();
                for (int Column = 0; Column < x + 1; Column++)
                {
                    Board[Column, Row] = Line[Column];
                }
            }
        }
    }

    private static void PlaceRandomShips(ref char[,] Board, ShipType[] Ships)
    {
        Random RandomNumber = new Random();
        bool Valid;
        char Orientation = ' ';
        int Row = 0;
        int Column = 0;
        int HorV = 0;
        for (int i = 0; i < shipno; i++)
        {
            Valid = false;
            while (Valid == false)
            {
                Row = RandomNumber.Next(0, y + 1);
                Column = RandomNumber.Next(0, x + 1);
                HorV = RandomNumber.Next(0, 2);
                if (HorV == 0)
                {
                    Orientation = 'v';
                }
                else
                {
                    Orientation = 'h';
                }
                Valid = ValidateBoatPosition(Board, Ships[i], Row, Column, Orientation);
            }
            Console.WriteLine("Computer placing the " + Ships[i].Name + " size: " + Ships[i].Size);
            int I = i;
            PlaceShip(ref Board, Ships, Row, Column, Orientation, i);
        }
        Console.Write("(press any to start)");
        Console.ReadKey();
    }

    private static void PlaceShip(ref char[,] Board, ShipType[] Ships, int Row, int Column, char Orientation, int I)
    {
        if (Orientation == 'v')
        {
            for (int Scan = 0; Scan < Ships[I].Size; Scan++)
            {
                Board[Column, Scan + Row] = Ships[I].Name[0];
            }
        }
        else if (Orientation == 'h')
        {
            for (int Scan = 0; Scan < Ships[I].Size; Scan++)
            {
                Board[Column + Scan, Row] = Ships[I].Name[0];
            }
        }
    }

    private static bool ValidateBoatPosition(char[,] Board, ShipType Ship, int Row, int Column, char Orientation)
    {
        if (Orientation == 'v' && (Row) + Ship.Size > y + 1)
        {
            return false;
        }
        else if (Orientation == 'h' && (Column) + Ship.Size > x + 1)
        {
            return false;
        }
        else
        {
            if (Orientation == 'v')
            {
                try
                {
                    for (int Scan = 0; Scan < Ship.Size; Scan++)
                    {
                        if (Board[Column, Scan + Row] != '-')
                        {
                            return false;
                        }
                    }
                }
                catch { return false; }
            }
            else if (Orientation == 'h')
            {
                try
                {
                    for (int Scan = 0; Scan < Ship.Size; Scan++)
                    {
                        if (Board[Column + Scan, Row] != '-')
                        {
                            return false;
                        }
                    }
                }
                catch { return false; }
            }
        }
        return true;
    }

    private static bool CheckWin(char[,] Board)
    {
        for (int Row = 0; Row < y + 1; Row++)
        {
            for (int Column = 0; Column < x + 1; Column++)
            {
                if (Board[Column, Row] == 'A' || Board[Column, Row] == 'B' || Board[Column, Row] == 'S' || Board[Column, Row] == 'D' || Board[Column, Row] == 'P' || Board[Column, Row] == '+')
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static void PrintBoard(char[,] Board)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.Write(" ");
        for (int Column = 0; Column < x + 1; Column++)
        {
            if (Column < 10)
            {
                Console.Write("  0" + Column);
            }
            else
            {
                Console.Write("  " + Column);
            }
        }
        Console.WriteLine();
        for (int Row = 0; Row < y + 1; Row++)
        {
            if (Row < 10)
            {
                Console.Write("0" + Row + " ");
            }
            else
            {
                Console.Write(Row + " ");
            }
            for (int Column = 0; Column < x + 1; Column++)
            {
                if (Board[Column, Row] == '-')
                {
                    Console.Write(" ");
                }
                else if (Board[Column, Row] == 'A' || Board[Column, Row] == 'B' || Board[Column, Row] == 'S' || Board[Column, Row] == 'D' || Board[Column, Row] == 'P')
                {
                    Console.Write(" ");
                }
                else if (Board[Column, Row] == '?')
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write(Board[Column, Row]);
                }
                else
                {
                    Console.Write(Board[Column, Row]);
                }
                Console.ResetColor();
                if (Column != x)
                {
                    Console.Write(" | ");
                    //Console.Write("-|-");
                    //Console.Write(" ║ ");
                }
            }
            Console.Write(" |\n");
        }
        Console.WriteLine();
    }

    private static void DisplayMenu(string name)
    {

        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("WELCOME TO WARSHIPS " + name + "\nMAIN MENU\n\n1. Start new game\n\n2. Change board dimensions\n\n3. Add a ship\n\n4. Change flare size\n\n5. Patches \n\n9. ragequit\n\n");

    }

    private static void Win(int Tries, string name)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        string chars = "Well Played " + name + "...............\nAttempts: " + Tries;
        foreach (char c in chars)
        {
            Console.Write(c);
            Thread.Sleep(20);
        }
        Thread.Sleep(2000);
        Console.ReadKey();
        Console.ResetColor();
    }

    private static int GetMainMenuChoice()
    {
        while (true)
        {
            try
            {
                char Choice = '0';
                Console.Write("Please enter your choice: ");
                Choice = Console.ReadKey().KeyChar;
                Console.WriteLine();
                return Choice;
            }
            catch (FormatException)
            {
                Console.WriteLine("enter numbers please! ");
            }
            catch
            {

            }
        }
    }

    private static void PlayGame(ref char[,] Board, ref ShipType[] Ships, ref int Tries)
    {
        bool GameWon = false;
        while (GameWon == false)
        {
            PrintBoard(Board);
            MakePlayerMove(ref Tries, ref Board, ref Ships);
            GameWon = CheckWin(Board);
            if (GameWon == true)
            {
                Win(Tries, name);
                Console.WriteLine();
                Console.ReadKey();

            }
        }
    }

    private static void SetUpShips(ref ShipType[] Ships, int defaultshipno/*, List<string> NewShipNames, List<int> NewShipSizes*/)
    {
        Ships[0].Name = "Aircraft Carrier";
        Ships[0].Size = 5;
        Ships[1].Name = "Battleship";
        Ships[1].Size = 4;
        Ships[2].Name = "Submarine";
        Ships[2].Size = 3;
        Ships[3].Name = "Destroyer";
        Ships[3].Size = 3;
        Ships[4].Name = "Patrol Boat";
        Ships[4].Size = 2;
        //if (shipno != defaultshipno)
        //{
        //    for (int i = 0; i < defaultshipno - shipno; i++)
        //    {
        //        Ships[5 + i].Name = NewShipNames[i];
        //        Ships[5 + i].Size = NewShipSizes[i];
        //    }
        //}
    }

    private static string AskName(ref string name)
    {
        while (true)
        {
            try
            {
                Console.Write("enter name: ");
                name = Console.ReadLine();
                break;
            }
            catch
            {
                Console.WriteLine("invalid, try again");
            }
        }
        Console.Clear();
        return name;
    }

    public static void CustomDimensions(ref int x, ref int y)
    {
        Console.Clear();
        Console.Write("enter x then y for board boundaries (9 - 99 recommended),\n");
        Console.Write("\nx: ");
        x = int.Parse(Console.ReadLine());
        Console.Write("\ny: ");
        y = int.Parse(Console.ReadLine());
    }

    public static void CustomShip(/*ref List<string> NewShipNames, ref List<int> NewShipSizes*/) //I honestly tried but this was too difficult and it made even more errors at every step
    {
        Console.Clear();
        Console.WriteLine("sorry this feature is currently unavailable... you can still edit the ships in the code\n(press any key to leave)");
        Console.ReadKey();
        //Console.Write("would you like to create a ship? (y/n): ");
        //bool choice = (Console.ReadKey().KeyChar == 'y');
        //    Console.Write("\nName: ");
        //    NewShipNames.Add(Console.ReadLine());
        //    int NewShipSize = 0;
        //while (!(NewShipSize > 0 && NewShipSize < x && NewShipSize < y))
        //{
        //    Console.Write("size (must be under x & y): ");
        //    NewShipSize = int.Parse(Console.ReadLine());
        //    if (!(NewShipSize > 0 && NewShipSize < x && NewShipSize < y))
        //    {
        //        NewShipSizes.Add(NewShipSize);
        //    }
        //}
        //shipno++;
    }

    public static String name = "";
    static void Main(string[] args)
    {
        //List<string> NewShipNames = new List<string>();
        //List<int> NewShipSizes = new List<int>();
        int defaultshipno = 5;
        AskName(ref name);
        int MenuOption = 0;
        while (MenuOption != 9 || (MenuOption > 5 || MenuOption < 1))
        {
            try
            {
                if (MenuOption == '1')
                {
                    ShipType[] Ships = new ShipType[shipno];
                    char[,] Board = new char[x + 1, y + 1];
                    SetUpBoard(ref Board);
                    SetUpShips(ref Ships, defaultshipno/*, NewShipNames, NewShipSizes*/);
                    PlaceRandomShips(ref Board, Ships);
                    Console.Clear();
                    PlayGame(ref Board, ref Ships, ref Tries);
                }
                else if (MenuOption == '2')
                {
                    CustomDimensions(ref x, ref y);
                    Console.Clear();
                }
                else if (MenuOption == '3')
                {
                    CustomShip(/*ref NewShipNames, ref NewShipSizes*/);
                    Console.Clear();
                }
                else if (MenuOption == '4')
                {
                    FlareSize = 0;
                    while (!(FlareSize == 3 || FlareSize == 5 || FlareSize == 7 || FlareSize == 9))
                    {
                        Console.Write("Enter flare size 3, 5, 7, 9: ");
                        FlareSize = Convert.ToInt32(Console.ReadKey().KeyChar);
                        Console.WriteLine();
                    }
                }
                else if (MenuOption == '5')
                {
                    Console.Clear();
                    string patches = "PATCHES\n\n-repeating input validation (all parts) \n\n-ask name\n\n-display shipsize on start\n\n-Show users attempts\n\n-validate column inputs\n\n-checksunk and display message\n\n-more validation\n\n-const to define number of ships, details need to be added to setupships first\n\n-resizable board 9 - 99 x and y\n\n-add custom ships\n\n(press any key to leave)";
                    foreach (char c1 in patches)
                    {
                        Console.Write(c1);
                        Thread.Sleep(2);
                    }
                    Console.ReadKey();
                }

                DisplayMenu(name);
                MenuOption = GetMainMenuChoice();
            }
            catch (FormatException)
            {
                Console.WriteLine("invalid type");
                Thread.Sleep(2000);
            }
            catch (IndexOutOfRangeException)
            {
                //part of the ship placing relies on an error that makes it have to repeat
            }
            catch (Exception)
            {
                Console.WriteLine("unknown error");
                Thread.Sleep(2000);
            }
            Tries = 0;
            Console.Clear();
        }
    }
}