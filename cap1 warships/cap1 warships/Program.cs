// edited Skeleton Program for the CAP1 examination
//this code should be used in conjunction with the Preliminary Material
//developed in a Visual Studio Community programming environment

//Version Number 1.1

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Runtime.Remoting.Services;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

class Program
{
    const int shipno = 6;
    static public int x = 10; //column
    static public int y = 10; //row

    public struct ShipType
    {
        public string Name;
        public int Size;
    }

    const string TrainingGame = "Training.txt";

    public List<int> previous = new List<int>();

    private static void GetRowColumn(ref int Row, ref int Column)
    {
        Console.WriteLine();
        bool valid = true;
        while (valid)
        {
            try
            {
                Console.Write("Please enter column: ");
                Column = Convert.ToInt32(Console.ReadLine());
                valid = false;
            }
            catch (FormatException)
            {
                Console.WriteLine("Try Again");
            }
            if (Column > x) { valid = true; Console.WriteLine("Try Again"); }
        }
        valid = true;
        while (valid)
        {
            try
            {
                Console.Write("Please enter row: ");
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
                Console.WriteLine(ships[i].Name + " was sunk");
                ships[i].Size = 1;
            }
        }
    }

    private static void MakePlayerMove(ref int Tries, ref char[,] Board, ref ShipType[] Ships)
    {
        int Row = y;
        int Column = x;
        GetRowColumn(ref Row, ref Column);
        Console.Clear();
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
            Console.Write(" Attempt: " + Tries);
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
                Board[Column + Scan, Row] = Ships[I].Name[0];
            }
        }
        else if (Orientation == 'h')
        {
            for (int Scan = 0; Scan < Ships[I].Size; Scan++)
            {
                Board[Column, Row + Scan] = Ships[I].Name[0];
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
                        if (Board[Row + Scan, Column] != '-')
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
                        if (Board[Column, Scan + Row] != '-')
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
                if (Board[Column, Row] == 'A' || Board[Column, Row] == 'B' || Board[Column, Row] == 'S' || Board[Column, Row] == 'D' || Board[Column, Row] == 'P')
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
        Console.WriteLine("The board looks like this: ");
        Console.WriteLine();
        Console.Write(" ");
        for (int Column = 0; Column < x + 1; Column++)
        {
            if (Column < 10)
            {
                Console.Write("0" + Column + "  ");
            }
            else
            {
                Console.Write(Column + "  ");
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
                else
                {
                    Console.Write(Board[Column, Row]);
                }
                if (Column != x)
                {
                    Console.Write(" | ");
                }
            }
            Console.WriteLine();
        }
    }

    private static void DisplayMenu(string name)
    {

        Console.CursorVisible = false;
        string msg1 = ("WELCOME TO WARSHIPS " + name + "\nMAIN MENU\n\n1. Start new game\n\n2. Start custom game\n\n3. #######\n\n9. ragequit\n\n");
        foreach (char c in msg1)
        {
            Console.Write(c);
            Thread.Sleep(1);
        }
        Thread.Sleep(100);
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("WELCOME TO WARSHIPS " + name + "\nMAIN MENU\n\n1. Start new game\n\n2. Start custom game\n\n3. Patches \n\n9. ragequit\n\n");

    }

    private static void Win(int Tries)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        string chars = "Well Played............... Attempts: " + Tries;
        foreach (char c in chars)
        {
            Console.Write(c);
            Thread.Sleep(1);
        }
        Console.ReadKey();
    }

    private static int GetMainMenuChoice()
    {
        while (true)
        {
            try
            {
                int Choice = 0;
                string chars = ("Please enter your choice: ");
                foreach (char c in chars)
                {
                    Console.Write(c);
                    Thread.Sleep(10);
                }
                Choice = Convert.ToInt32(Console.ReadLine());
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

    private static void PlayGame(ref char[,] Board, ref ShipType[] Ships)
    {
        bool GameWon = false;
        int Tries = 0;
        while (GameWon == false)
        {
            PrintBoard(Board);
            MakePlayerMove(ref Tries, ref Board, ref Ships);
            GameWon = CheckWin(Board);
            if (GameWon == true)
            {
                Win(Tries);
                Console.WriteLine();
                Console.ReadKey();

            }
        }
    }

    private static void SetUpShips(ref ShipType[] Ships)
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
        Ships[5].Name = "Buoy";
        Ships[5].Size = 1;
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

    public static void Customise(ref int x, ref int y)
    {
        Console.Clear();
        Console.Write("enter x then y for board boundaries,\nx: ");
        x = int.Parse(Console.ReadLine());
        Console.Write("y: ");
        y = int.Parse(Console.ReadLine());
    }

    static void Main(string[] args)
    {
        String name = "";
        Console.WriteLine("Before we start would you like to customise? (y/n)");
        char c = Console.ReadKey().KeyChar;
        if (c == 'y') { Customise(ref x, ref y); }
        Console.Clear();
        ShipType[] Ships = new ShipType[shipno];
        char[,] Board = new char[x + 1, y + 1];
        AskName(ref name);
        int MenuOption = 0;
        while (MenuOption != 9 || (MenuOption > 2 && MenuOption < 1))
        {
            //try
            //{
                SetUpBoard(ref Board);
                SetUpShips(ref Ships);
                if (MenuOption == 1)
                {
                    PlaceRandomShips(ref Board, Ships);
                    Console.Clear();
                    PlayGame(ref Board, ref Ships);
                }
                else if (MenuOption == 3)
                {
                    Console.Clear();
                    string patches = "PATCHES\n\n-repeating input validation (all parts) \n\n-ask name\n\n-display shipsize on start\n\n-Show users attempts\n\n-validate column inputs\n\n-checksunk and display message\n\n-more validation\n\n-const to define number of ships, details need to be added to setupships first\n\n-resizable board 1-100 x and y\n\n-added buoy........good luck (size: 1)";
                    foreach (char c1 in patches)
                    {
                        Console.Write(c1);
                        Thread.Sleep(2);
                    }
                    Console.ReadKey();
                }

                DisplayMenu(name);
                MenuOption = GetMainMenuChoice();
            //}
            //catch (FormatException)
            //{
            //    Console.Write("sisyphus");
            //}
            //catch (IndexOutOfRangeException)
            //{
            //    Console.WriteLine("........................");
            //}
            //catch (Exception)
            //{
            //    Console.WriteLine("grrrrr");
            //}
            Console.Clear();
        }
    }
}