using BattleshipMaxConsole.Data;
using System;

namespace BattleshipMaxConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(95, 45);
            var game = new Game();
            Console.WriteLine("Hello welcome to MarreNCharres battleship game!");
            PrintGrid(game.CreatePlayerGrid(), game.CreateEnemyGrid());
            Console.WriteLine("Enter your name!!!!!");
            var nameOfPlayer = Console.ReadLine();
            game.User = nameOfPlayer;
            Console.WriteLine("Host adress!!! if you leave this empty you agree to be da host");
            var hostAdress = Console.ReadLine();
            Console.WriteLine("Port:");
            var hostPort = Console.ReadLine();
            game.Play(hostAdress, hostPort);
            Console.ReadLine();

        }

        //Printar ut boardet!
        public static void PrintGrid(string[,] playerGrid, string[,] enemyGrid)
        {
            Console.WriteLine();
            Console.WriteLine("                MY FIELD                                            ENEMY FIELD");
            Console.WriteLine("   1   2   3   4   5   6   7   8   9   10             1   2   3   4   5   6   7   8   9   10");
            Console.WriteLine("  ---------------------------------------            ---------------------------------------");
            for (int i = 0; i < 10; i++)
            {
                switch (i)
                {
                    case 0:
                        Console.Write("A");
                        break;
                    case 1:
                        Console.Write("B");
                        break;
                    case 2:
                        Console.Write("C");
                        break;
                    case 3:
                        Console.Write("D");
                        break;
                    case 4:
                        Console.Write("E");
                        break;
                    case 5:
                        Console.Write("F");
                        break;
                    case 6:
                        Console.Write("G");
                        break;
                    case 7:
                        Console.Write("H");
                        break;
                    case 8:
                        Console.Write("I");
                        break;
                    case 9:
                        Console.Write("J");
                        break;
                    default:
                        Console.Write("");
                        break;
                }
                Console.Write("|");
                for (int j = 0; j < 10; j++)
                {

                    Console.Write(" ");
                    if (playerGrid[i, j] != "~")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(playerGrid[i, j]);
                    }
                    else if (playerGrid[i, j] == "X")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(playerGrid[i, j]);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(playerGrid[i, j]);
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(" ");
                    Console.Write("|");

                }
                Console.Write("         ");
                switch (i)
                {
                    case 0:
                        Console.Write("A");
                        break;
                    case 1:
                        Console.Write("B");
                        break;
                    case 2:
                        Console.Write("C");
                        break;
                    case 3:
                        Console.Write("D");
                        break;
                    case 4:
                        Console.Write("E");
                        break;
                    case 5:
                        Console.Write("F");
                        break;
                    case 6:
                        Console.Write("G");
                        break;
                    case 7:
                        Console.Write("H");
                        break;
                    case 8:
                        Console.Write("I");
                        break;
                    case 9:
                        Console.Write("J");
                        break;
                    default:
                        Console.Write("");
                        break;
                }
                Console.Write("|");
                for (int j = 0; j < 10; j++)
                {

                    Console.Write(" " + enemyGrid[i, j] + " ");
                    Console.Write("|");

                }
                Console.WriteLine();
                Console.Write(" |---------------------------------------|          |---------------------------------------|");

                Console.WriteLine();
            }
            Console.WriteLine("_______________________________________________________________________________________________");

        }
    }
}
