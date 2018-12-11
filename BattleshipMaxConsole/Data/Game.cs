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
        Dictionary<int, string> responses = new Dictionary<int, string>()
        {
            {210,"210 BATTLESHIP/1.0" },
            {220, "220 <remote player name>" },
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

        public string User { get; set; }

        public string[,] CreatePlayerGrid()
        {
            var grid = new string[10, 10];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    var currentCoordinate = i + "." + j;
                    var shipDetected = _ships.Where(x => x.Coordinates.Contains(currentCoordinate)).FirstOrDefault();

                    if (shipDetected != null)
                    {
                        var nameArray = shipDetected.Name.ToCharArray();

                        grid[i, j] = $"{nameArray[0]}";
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
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Failed to open socket. Probably already taken.");
                Environment.Exit(1);
            }
        }
        public void Host()
        {
            while (true)
            {
                Console.WriteLine("Waiting for opponent...");
                

                using (var client = _listener.AcceptTcpClient())
                using (var networkStream = client.GetStream())
                using (StreamReader reader = new StreamReader(networkStream, Encoding.UTF8))
                using (var writer = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true })
                {
                    Console.WriteLine($"Client has connected {client.Client.RemoteEndPoint}!");
                    writer.WriteLine(responses[210]);
                    TcpMessage(true, responses[210]);
                    
                    while (client.Connected)
                    {
                        var command = reader.ReadLine();
                        TcpMessage(false, command);
                        writer.WriteLine("220 " + User);
                        TcpMessage(true, "220 " + User);


                        if (string.Equals(command, "EXIT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            writer.WriteLine("BYE BYE");
                            break;
                        }

                        if (string.Equals(command, "DATE", StringComparison.InvariantCultureIgnoreCase))
                        {
                            writer.WriteLine(DateTime.UtcNow.ToString("o"));
                            break;
                        }

                        writer.WriteLine($"UNKNOWN COMMAND: {command}");
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
                Console.WriteLine($"Connected to {client.Client.RemoteEndPoint}");
                while (client.Connected)
                {
                    Console.WriteLine("Enter text to send: (Write QUIT to end plix)");
                    var text = Console.ReadLine();

                    if (text == "QUIT") break;

                    // Skicka text
                    writer.WriteLine(text);

                    if (!client.Connected) break;

                    // Läs minst en rad
                    do
                    {
                        var line = reader.ReadLine();
                        Console.WriteLine($"Answere: {line}");

                    } while (networkStream.DataAvailable);

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
            Console.WriteLine(message);
            Console.ResetColor();

        }
    }

}
