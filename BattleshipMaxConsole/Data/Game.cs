using BattleshipMaxConsole.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BattleshipMaxConsole.Data
{
    public class Game
    {
        public static ConsoleMessages SavedMessages = new ConsoleMessages();
        public static Player ThePlayer = new Player();
        public static Player TheOpponent = new Player();

        Dictionary<int, string> responses = new Dictionary<int, string>()
        {
            {210, "210 BATTLESHIP/1.0" },
            {220, "220 "  },
            {221, "221 Client Starts" },
            {222, "222 Host Starts" },
            {230, "230 Miss!" },
            {241, "241 You hit my Carrier" },
            {242, "242 You hit my Battleship" },
            {243, "243 You hit my Destroyer" },
            {244, "244 You hit my Submarine" },
            {245, "245 You hit my Patrol Boat" },
            {251, "251 You sunk my Carrier" },
            {252, "252 You sunk my Battleship" },
            {253, "253 You sunk my Destroyer" },
            {254, "254 You sunk my Submarine" },
            {255, "255 You sunk my Patrol Boat" },
            {260, "260 You win!" },
            {270, "270 Connection closed" },
            {500, "500 Syntax error" },
            {501, "501 Sequence error" }
        };
        TcpListener _listener;
        private List<Ship> _ships = new List<Ship>()
        {
            new Ship()
            {
                Name = "Carrier",
                Health = 5,
                Coordinates = new string[]{ "0.3", "0.4", "0.5", "0.6", "0.7" }

            },
            new Ship()
            {
                Name = "Battleship",
                Health = 4,
                Coordinates = new string[]{"1.6","1.7","1.8","1.9"}

            },
            new Ship()
            {
                Name = "Destroyer",
                Health = 3,
                Coordinates = new string[]{"3.3","3.4","3.5"}
            },
            new Ship()
            {
                Name = "Submarine",
                Health = 3,
                Coordinates = new string[]{"5.1","5.2","5.3"}
            },
            new Ship()
            {
                Name = "Patrol Boat",
                Health = 2,
                Coordinates = new string[]{"6.3","6.4"}
            },
        };
        private List<string> _targets = new List<string>();
        private List<string> _confirmedHits = new List<string>();
        private List<string> _confirmedMisses = new List<string>();
        private int PlayerHealth = 17;
        public string User { get; set; }
        public string Opponent { get; set; }
        public string[,] CreatePlayerGrid()
        {
            var grid = new string[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var currentCoordinate = i + "." + j;
                    var hit = _targets.Where(x => x == currentCoordinate);

                    var shipDetected = _ships.Where(x => x.Coordinates.Contains(currentCoordinate)).FirstOrDefault();

                    if (hit != null)
                    {
                        grid[i, j] = "H";
                    }
                    if (shipDetected != null)
                    {
                        var nameArray = shipDetected.Name.ToCharArray();

                        grid[i, j] = $"{nameArray[0]}";
                    }
                    else if (shipDetected != null && hit != null)
                    {
                        shipDetected.Health--;
                        grid[i, j] = "X";
                    }
                    else
                    {

                        grid[i, j] = "~";

                    }
                }
            }

            return grid;
        }
        public string[,] CreateEnemyGrid()
        {
            var grid = new string[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    //hit or miss
                    grid[i, j] = "~";
                }
            }

            return grid;
        }
        public void StartListen(int port)
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, port);
                _listener.Start();
                Console.WriteLine($"Starts listening on port: {port}");
                Game.SavedMessages.AddMessage($"Starts listening on port: {port}\r\n", false);
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Failed to open socket. Probably already taken.");
                Game.SavedMessages.AddMessage($"Failed to open socket. Probably already taken.\r\n", false);
                Environment.Exit(1);
            }
        }
        public void Host()
        {
            while (true)
            {

                Console.WriteLine("Waiting for opponent...");
                Game.SavedMessages.AddMessage($"Waiting for opponent...\r\n", false);

                using (var client = _listener.AcceptTcpClient())
                using (var networkStream = client.GetStream())
                using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    var errorMessages = 0;
                    var turn = 0;
                    Console.WriteLine($"Client has connected {client.Client.RemoteEndPoint}!");
                    Game.SavedMessages.AddMessage($"Client has connected {client.Client.RemoteEndPoint}!\r\n", false);

                    writer.WriteLine(responses[210]);
                    TcpMessage(false, responses[210]);

                    while (client.Connected)
                    {
                        turn++;

                        var command = reader.ReadLine();
                        Game.SavedMessages.AddMessage($"{command}\r\n", false);

                        if (command.Length < 3)
                        {
                            
                            writer.WriteLine(responses[500]);
                            continue;
                        }
                        

                        if (turn == 1)
                        {
                            TcpMessage(false, "HELLO " + User);
                            Opponent = command.Substring(3);
                            writer.WriteLine("220 " + User);
                            TcpMessage(true, "220 " + Opponent);

                        }
                        else
                        {
                            //if (command.Substring(0, 5) != "START")
                            //{
                            //    TcpMessage(true, command);
                            //}
                            int code;


                            if (string.Equals(command, "START", StringComparison.InvariantCultureIgnoreCase))
                            {
                                errorMessages = 0;
                                //TODO: fix logic for randomize player to start!!!
                                Random rand = new Random();


                                if (rand.NextDouble() >= 0.5)
                                {
                                    var uStartMessage = "222 I " + User + ", will start!";
                                    //222
                                    //You start
                                    writer.WriteLine(uStartMessage);
                                    TcpMessage(false, uStartMessage);
                                    command = Console.ReadLine();
                                    Game.SavedMessages.AddMessage($"{command}\r\n", false);
                                    writer.WriteLine(command);
                                    var answere = reader.ReadLine();
                                    TcpMessage(true, answere);
                                    continue;
                                }
                                else
                                {
                                    //221
                                    //Opponent start
                                    var oStartMessage = "221 You, player " + Opponent + ", will start!";
                                    writer.WriteLine(oStartMessage);
                                    TcpMessage(false, "Opponent " + Opponent + ", will start!");
                                    continue;
                                }

                            }

                            if (string.Equals(command.Substring(0, 4), "FIRE", StringComparison.InvariantCultureIgnoreCase))
                            {
                                errorMessages = 0;
                                //check if hit????
                                var response = ConvertAndAddTarget(command.Substring(5, 2),"player");
                                //Add target
                                writer.WriteLine(response);
                                TcpMessage(true, command);
                                TcpMessage(false, response);
                                command = Console.ReadLine();
                                Game.SavedMessages.AddMessage($"{command}\r\n", false);
                                writer.WriteLine(command);
                                //TcpMessage(false, command);
                                continue;

                            }

                            if (int.TryParse(command.Substring(0, 3), out code))
                            {
                                if (code == 220)
                                {
                                    continue;
                                }

                                if (code == 500 || code == 501)
                                {
                                    errorMessages++;
                                    if (errorMessages == 3)
                                    {
                                        Console.WriteLine("You sir have failed! Closing connection!!!");
                                        Game.SavedMessages.AddMessage($"You sir have failed! Closing connection!!!\r\n", false);
                                        client.Close();
                                        Console.ReadLine();
                                        break;
                                    }
                                    TcpMessage(true, responses[code]);
                                    var newtry = Console.ReadLine();
                                    writer.WriteLine(newtry);
                                    continue;
                                }
                                errorMessages = 0;
                                //IF MATCH??? write message
                                TcpMessage(true, responses[code]);
                                continue;
                            }


                            if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
                            {
                                //TODO: Shut down!
                                writer.WriteLine(responses[270]);
                                TcpMessage(true, responses[270]);
                                client.Close();
                                break;
                            }

                            errorMessages++;
                            writer.WriteLine(responses[500]);
                            //TcpMessage(true, responses[500]);
                        }
                    }
                }

            }

        }
        public void ConnectToServer(string hostAdress, string hostPort)
        {
            using (var client = new TcpClient(hostAdress, int.Parse(hostPort)))
            using (var networkStream = client.GetStream())
            using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
            using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
            {
                var errorMessages = 0;
                var turn = 0;
                int code;
                Console.WriteLine($"Connected to {client.Client.RemoteEndPoint}");
                Game.SavedMessages.AddMessage($"Connected to {client.Client.RemoteEndPoint}\r\n", false);
                TcpMessage(true, reader.ReadLine());
                writer.WriteLine("220 " + User);
                TcpMessage(false, "HELLO " + User);


                try
                {
                    var text1 = reader.ReadLine();
                    Game.SavedMessages.AddMessage($"{text1}\r\n", false);
                    Opponent = text1.Split()[1];
                    TcpMessage(true, text1);
                }
                catch (Exception)
                {
                    Opponent = "(Host)";
                    TcpMessage(true, "220 " + Opponent);
                }


                while (client.Connected)
                {
                    turn++;
                    if (turn == 1)
                    {
                        Console.WriteLine("Write 'START' to start the game");
                        Game.SavedMessages.AddMessage($"Write 'START' to start the game\r\n", false);
                        var input = Console.ReadLine().ToUpper();
                        Game.SavedMessages.AddMessage($"{input}\r\n", false);
                        //TcpMessage(false, input);
                        writer.WriteLine(input);

                    }
                    var command = reader.ReadLine();
                    Game.SavedMessages.AddMessage($"{command}\r\n", false);
                    //TcpMessage(false, command);

                    if (command.Length < 3)
                    {
                        errorMessages++;
                        if (errorMessages == 3)
                        {
                            Console.WriteLine("You sir have failed! Closing connection!!!");
                            Game.SavedMessages.AddMessage($"You sir have failed! Closing connection!!!\r\n", false);
                            client.Close();
                            Console.ReadLine();
                            break;
                        }
                        writer.WriteLine(responses[500]);
                        continue;
                    }

                    if (command.Substring(0, 3) == "221")
                    {
                        //Client starts
                        TcpMessage(true, command);
                        var clientInput = Console.ReadLine();
                        writer.WriteLine(clientInput);
                        //TcpMessage(false, clientInput);
                        continue;

                    }
                    else if (command.Substring(0, 3) == "222")
                    {
                        //Host starts
                        TcpMessage(true, responses[222]);
                        command = reader.ReadLine();
                        Game.SavedMessages.AddMessage($"{command}\r\n", false);

                        if (command.Length < 3)
                        {
                            errorMessages++;
                            if (errorMessages == 3)
                            {
                                Console.WriteLine("You sir have failed! Closing connection!!!");
                                Game.SavedMessages.AddMessage($"You sir have failed! Closing connection!!!\r\n", false);
                                client.Close();
                                Console.ReadLine();
                                break;
                            }
                            writer.WriteLine(responses[500]);
                            continue;
                        }

                    }

                    if (string.Equals(command.Substring(0, 4), "FIRE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        errorMessages = 0;
                        //check if hit????
                        var response = ConvertAndAddTarget(command.Substring(5, 2), "player");
                        //Add target
                        writer.WriteLine(response);
                        TcpMessage(true, command);
                        TcpMessage(false, response);
                        command = Console.ReadLine();
                        Game.SavedMessages.AddMessage($"{command}\r\n", false);
                        writer.WriteLine(command);
                        //TcpMessage(false, command);
                        continue;
                    }

                    if (int.TryParse(command.Substring(0, 3), out code))
                    {
                        if (code == 220)
                        {
                            continue;
                        }

                        if (code == 500 || code == 501)
                        {
                            errorMessages++;
                            if (errorMessages == 3)
                            {
                                Console.WriteLine("You sir have failed! Closing connection!!!");
                                Game.SavedMessages.AddMessage($"You sir have failed! Closing connection!!!\r\n", false);
                                client.Close();
                                Console.ReadLine();
                                break;
                            }
                            TcpMessage(true, responses[code]);
                            var newtry = Console.ReadLine();
                            writer.WriteLine(newtry);
                            continue;
                        }

                        errorMessages = 0;
                        //IF MATCH??? write message
                        TcpMessage(true, responses[code]);
                        continue;
                    }

                    if (string.Equals(command, "QUIT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //TODO: Shut down!
                        writer.WriteLine(responses[270]);
                        TcpMessage(true, responses[270]);
                        client.Close();
                        break;
                    }

                    errorMessages++;
                    writer.WriteLine(responses[500]);
                    //TcpMessage(true, responses[500]);

                };
            }
        }
        public void Play(string hostAdress, string hostPort)
        {
            if (hostAdress == "")
            {
                if (hostPort != "")
                {
                    StartListen(int.Parse(hostPort));
                    Host();
                }
                else
                {
                    Console.WriteLine("Something went reeaaally wrong because you did not enter port");
                    Game.SavedMessages.AddMessage($"Something went reeaaally wrong because you did not enter port\r\n", false);
                }
            }
            else if (hostAdress != "" && hostPort != "")
            {
                ConnectToServer(hostAdress, hostPort);
            }
        }

        public void TcpMessage(bool blue, string message)
        {
            if (blue)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
            }
            Game.SavedMessages.AddMessage($"{message}\r\n", blue);
            Console.WriteLine(message);
            Console.ResetColor();

        }
        public string ConvertAndAddTarget(string target, string player)
        {
            PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
            //Lägg till targets
            var targetToAdd = ParseLocation(target);
            var shipHit = _ships.Where(s => s.Coordinates.Contains(targetToAdd)).FirstOrDefault();
            var cord = targetToAdd.Split('.');

            if (_targets.Contains(targetToAdd))
            {
                //target already exists????

                return "503";

            }
            _targets.Add(targetToAdd);
            if (shipHit == null)
            {
                if (player == "player")
                {
                    Player.Grid[int.Parse(cord[0]), int.Parse(cord[1])] = "M";
                }
                else
                {
                    Player.OpponentGrid[int.Parse(cord[0]), int.Parse(cord[1])] = "M";
                }
            }

            if (shipHit != null)
            {
                if (player == "player")
                {
                    Player.Grid[int.Parse(cord[0]), int.Parse(cord[1])] = "H";
                }
                else
                {
                    Player.OpponentGrid[int.Parse(cord[0]), int.Parse(cord[1])] = "H";
                }

                PlayerHealth--;
                if (PlayerHealth == 0)
                {
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[260];
                }

                if (shipHit.Name == "Carrier")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                        return responses[241];

                    }
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[251];

                }
                if (shipHit.Name == "Battleship")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                        return responses[242];

                    }
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[252];
                }
                if (shipHit.Name == "Destroyer")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                        return responses[243];

                    }
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[253];
                }
                if (shipHit.Name == "Submarine")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                        return responses[244];

                    }
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[254];
                }
                if (shipHit.Name == "Patrol Boat")
                {
                    shipHit.Health--;
                    if (shipHit.Health != 0)
                    {
                        PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                        return responses[245];

                    }
                    PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
                    return responses[255];
                }
            }
            PrintGrid(Player.Grid, Player.OpponentGrid, ThePlayer, TheOpponent);
            return responses[230];
        }
        public string ParseLocation(string location)
        {
            var i = location.Substring(0, 1);
            var rest = "";
            if (location.Length > 2)
            {
                rest = "." + (int.Parse(location.Substring(1, 2)) - 1);
            }
            else
            {
                rest = "." + (int.Parse(location.Substring(1, 1)) - 1);
            }

            switch (i.ToUpper())
            {
                case "A":
                    return "0" + rest;
                case "B":
                    return "1" + rest;
                case "C":
                    return "2" + rest;
                case "D":
                    return "3" + rest;
                case "E":
                    return "4" + rest;
                case "F":
                    return "5" + rest;
                case "G":
                    return "6" + rest;
                case "H":
                    return "7" + rest;
                case "I":
                    return "8" + rest;
                case "J":
                    return "9" + rest;
                default:
                    return "false location woot";
            }
        }

        //Printar ut boardet!
        public static void PrintGrid(string[,] grid, string[,] enemyGrid, Player player, Player opponent)
        {
            Console.Clear();
            Console.WriteLine($"Me: Name:{player.Name} Address:{player.HostAddress} Port:{player.Port} | Enemy: Name:{opponent.Name} Address:{opponent.HostAddress}  Port:{opponent.Port}");
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

                    Console.Write(" " + grid[i, j] + " ");
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
            Console.WriteLine();

            foreach (var message in Game.SavedMessages.PrintedMessages)
            {
                if (message.IsBlue)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.Write(message.Text);
                Console.ResetColor();
            }
        }
    }
}
